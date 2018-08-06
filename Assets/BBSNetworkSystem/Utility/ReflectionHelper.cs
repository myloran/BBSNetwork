using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Entities;

public delegate Entity SpawnDelegate(EntityManager entityManager);
internal delegate void SetterDelegate<S, T>(ref S instance, T value);
internal delegate T GetterDelegate<S, T>(ref S instance);

internal abstract class NetworkField { }

internal abstract class NetworkField<OBJ> : NetworkField {
  public readonly SyncBaseAttribute syncAttribute;

  public NetworkField(SyncBaseAttribute syncAttribute) {
    this.syncAttribute = syncAttribute;
  }

  public abstract void SetValue(
    ref OBJ obj, 
    int oldValue, 
    int newValue, 
    float deltaTimeFrame, 
    float deltaTimeMessage);

  public abstract int GetValue(OBJ obj);
}

internal sealed class NetworkField<OBJ, TYPE> : NetworkField<OBJ> {
  public readonly GetterDelegate<OBJ, TYPE> GetValueDelegate;
  public readonly SetterDelegate<OBJ, TYPE> SetValueDelegate;
  //private readonly MemberInfo memberInfo;
  readonly NetworkField parent;
  NetworkMath networkMath;

  public NetworkField(
    MemberInfo info,
    SyncBaseAttribute syncAttribute) : base(syncAttribute
  ) {
    //Debug.Log(typeof(OBJ) + " --- " + typeof(TYPE));
    if (info.MemberType == MemberTypes.Field) {
      var fieldInfo = (FieldInfo)info;
      GetValueDelegate = PropertyReflection.CreateGetter<OBJ, TYPE>(fieldInfo);
      SetValueDelegate = PropertyReflection.CreateSetter<OBJ, TYPE>(fieldInfo);

    } else if (info.MemberType == MemberTypes.Property) {
      var propertyInfo = (PropertyInfo)info;

      GetValueDelegate = (GetterDelegate<OBJ, TYPE>)Delegate.CreateDelegate(
        typeof(Func<OBJ, TYPE>),
        propertyInfo.GetGetMethod());

      SetValueDelegate = (SetterDelegate<OBJ, TYPE>)Delegate.CreateDelegate(
        typeof(Action<OBJ, TYPE>),
        propertyInfo.GetSetMethod());

    } else { 
      throw new NotImplementedException(info.MemberType.ToString());
    }
    var typeType = typeof(TYPE);

    if (typeType == typeof(int)) {
      networkMath = new NetworkMathInteger();

    } else if (typeType == typeof(float)) {
      networkMath = new NetworkMathFloat(
        syncAttribute.Accuracy,
        syncAttribute.LerpSpeed,
        syncAttribute.JumpThreshold);

    } else if (typeType == typeof(boolean)) {
      networkMath = new NetworkMathBoolean();
    }   
  }

  public NetworkField(
      MemberInfo info, 
      NetworkField parent,
      SyncBaseAttribute syncAttribute) : base(syncAttribute
  ) {
    this.parent = parent;
  }

  public override void SetValue(
      ref OBJ obj, 
      int oldValue, 
      int newValue, 
      float deltaTimeFrame, 
      float deltaTimeMessage
  ) {
    var type = (TYPE)networkMath.IntegerToNative(
      GetValueDelegate(ref obj), 
      oldValue, 
      newValue, 
      deltaTimeFrame, 
      deltaTimeMessage);

    SetValueDelegate(ref obj, type);
  }

  public override int GetValue(OBJ obj) {
    return networkMath.NativeToInteger(GetValueDelegate(ref obj));
  }
}

internal sealed class NetworkField<Parent_OBJ, OBJ, TYPE> : NetworkField<Parent_OBJ> {
  public readonly GetterDelegate<OBJ, TYPE> Getter;
  public readonly SetterDelegate<OBJ, TYPE> Setter;
  //private readonly MemberInfo memberInfo;
  readonly NetworkField<Parent_OBJ, OBJ> parent;
  NetworkMath networkMath;

  public NetworkField(
      MemberInfo info,
      NetworkField parent,
      SyncBaseAttribute syncAttribute) : base(syncAttribute
  ) {
    this.parent = (NetworkField<Parent_OBJ, OBJ>)parent;
    //Debug.Log(typeof(Parent_OBJ) + " --- " + typeof(OBJ) + " --- " + typeof(TYPE));
    if (info.MemberType == MemberTypes.Field) {
      var fieldInfo = (FieldInfo)info;
      Getter = PropertyReflection.CreateGetter<OBJ, TYPE>(fieldInfo);
      Setter = PropertyReflection.CreateSetter<OBJ, TYPE>(fieldInfo);

    } else if (info.MemberType == MemberTypes.Property) {
      var propertyInfo = (PropertyInfo)info;

      Getter = (GetterDelegate<OBJ, TYPE>)Delegate.CreateDelegate(
        typeof(GetterDelegate<OBJ, TYPE>), 
        propertyInfo.GetGetMethod());

      Setter = (SetterDelegate<OBJ, TYPE>)Delegate.CreateDelegate(
        typeof(SetterDelegate<OBJ, TYPE>), 
        propertyInfo.GetSetMethod());

    } else {
      throw new NotImplementedException(info.MemberType.ToString());
    }

    var typeType = typeof(TYPE);

    if (typeType == typeof(int)) {
      networkMath = new NetworkMathInteger();

    } else if (typeType == typeof(float)) {
      networkMath = new NetworkMathFloat(
        syncAttribute.Accuracy, 
        syncAttribute.LerpSpeed, 
        syncAttribute.JumpThreshold);

    } else if (typeType == typeof(boolean)) {
      networkMath = new NetworkMathBoolean();
    }
  }

  public override void SetValue(
      ref Parent_OBJ parentObj, 
      int oldValue, 
      int newValue, 
      float deltaTimeFrame, 
      float deltaTimeMessage
  ) {
    OBJ obj = parent.GetValueDelegate(ref parentObj);

    var type = (TYPE)networkMath.IntegerToNative(
      Getter(ref obj), 
      oldValue, 
      newValue, 
      deltaTimeFrame, 
      deltaTimeMessage);

    Setter(ref obj, type);
    parent.SetValueDelegate(ref parentObj, obj);
  }

  public override int GetValue(Parent_OBJ parentObj) {
    OBJ obj = parent.GetValueDelegate(ref parentObj);

    return networkMath.NativeToInteger(Getter(ref obj));
  }
}

internal sealed class NetworkMethod<T> {
  readonly Action<T> actionDelegate;

  public NetworkMethod(MethodInfo info) {
    actionDelegate = (Action<T>)Delegate.CreateDelegate(
      typeof(Action<T>), 
      info);
  }

  public void Invoke(T obj) {
    actionDelegate(obj);
  }
}

internal sealed class NetworkMethodInfo<T, Param> {
  readonly Action<T, Param> actionDelegate;

  public NetworkMethodInfo(MethodInfo info) {
    actionDelegate = (Action<T, Param>)Delegate.CreateDelegate(
      typeof(Action<T, Param>), 
      info);
  }

  public void Invoke(T obj, Param arg) {
    actionDelegate(obj, arg);
  }
}

internal sealed class NetworkMethodInfo<T, Param1, Param2> {
  readonly Action<T, Param1, Param2> actionDelegate;

  public NetworkMethodInfo(MethodInfo info) {
    actionDelegate = (Action<T, Param1, Param2>)Delegate.CreateDelegate(
      typeof(Action<T, Param1, Param2>), 
      info);
  }

  public void Invoke(T obj, Param1 arg1, Param2 arg2) {
    actionDelegate(obj, arg1, arg2);
  }
}

internal sealed class NetworkInOutMethodInfo<T, RefParam, OutParam> {
  internal delegate bool NetworkInOutDelegate(
    T obj, 
    ref RefParam refArg, 
    out OutParam outArg);

  readonly NetworkInOutDelegate functionDelegate;

  public NetworkInOutMethodInfo(MethodInfo info) {
    functionDelegate = (NetworkInOutDelegate)Delegate.CreateDelegate(
      typeof(NetworkInOutDelegate), 
      info);
  }

  public bool Invoke(T obj, ref RefParam refArg, out OutParam outArg) {
    return functionDelegate(obj, ref refArg, out outArg);
  }
}


internal static class PropertyReflection {
  public static GetterDelegate<S, T> CreateGetter<S, T>(FieldInfo field) {
    var instance = Expression.Parameter(
      typeof(S).MakeByRefType(), 
      "instance");

    var memberAccess = Expression.MakeMemberAccess(
      instance, 
      field);

    var expr = Expression.Lambda<GetterDelegate<S, T>>(
      memberAccess, 
      instance);

    return expr.Compile();
  }

  public static SetterDelegate<S, T> CreateSetter<S, T>(FieldInfo field) {
    var instance = Expression.Parameter(
      typeof(S).MakeByRefType(), 
      "instance");

    var value = Expression.Parameter(
      typeof(T), 
      "value");

    var assign = Expression.Assign(
      Expression.Field(instance, field),
      Expression.Convert(value, field.FieldType));

    var expr = Expression.Lambda<SetterDelegate<S, T>>(
      assign, 
      instance, 
      value);

    return expr.Compile();
  }
}

//TODO: Make one instance
internal class ReflectionUtility {
  public readonly ComponentType[] ComponentTypes;
  Dictionary<ComponentType, NetworkField[]> cashedFields = new Dictionary<ComponentType, NetworkField[]>();
  readonly Dictionary<ComponentType, int> typeIds = new Dictionary<ComponentType, int>();
  readonly Dictionary<int, ComponentType> idTypes = new Dictionary<int, ComponentType>();
  readonly Dictionary<ComponentType, int> FieldCounts = new Dictionary<ComponentType, int>();
  //public static readonly MethodInfo[] NetworkFactoryMethods;
  Dictionary<int, SpawnDelegate> spawns = new Dictionary<int, SpawnDelegate>();
  byte id;

  public ReflectionUtility() {
    //var networkFactoryMethods = new List<MethodInfo>();
    var assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
    assemblies.Sort((x, y) => x.FullName.CompareTo(y.FullName));

    id = 0;
    var types = new List<ComponentType>();

    foreach (var assembly in assemblies) {
      types.AddRange(
        RegisterAttributes(assembly));
    }
    ComponentTypes = types.ToArray();
    //NetworkFactoryMethods = networkFactoryMethods.ToArray();
  }

  public ReflectionUtility(Assembly assembly) {
    id = 0;
    ComponentTypes = RegisterAttributes(assembly).ToArray();
  }

  IList<ComponentType> RegisterAttributes(Assembly assembly) {
    var types = new List<Type>(assembly.GetTypes());
    types.Sort((x, y) => x.Name.CompareTo(y.Name));
    var componentTypes = new List<ComponentType>();

    foreach (var type in types) {
      if (type.GetCustomAttribute<SyncAttribute>() != null) {
        var fields = FindFields(type);
        id++;
        typeIds.Add(type, id);
        idTypes.Add(id, type);
        cashedFields.Add(type, fields.ToArray());
        FieldCounts.Add(type, fields.Count);
        componentTypes.Add(type);
      }

      FindSpawns(type);
    }

    return componentTypes;
  }

  void FindSpawns(Type type) {
    if (type.GetCustomAttribute<SpawnFactoryAttribute>() != null) {
      //networkFactoryMethods.AddRange(type.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(NetworkInstantiatorAttribute), false)));
      var methods = type
        .GetMethods()
        .Where(_ => _.IsDefined(typeof(SpawnAttribute), false))
        .ToArray();

      foreach (var method in methods) {
        SpawnDelegate spawn = null;

        try {
          spawn = (SpawnDelegate)Delegate.CreateDelegate(
            typeof(SpawnDelegate),
            method);
        } catch (Exception) {
          throw new Exception(string.Format("Wrong signature for {0}. Signature requires static Entity {0}(EntityManager)", method.Name));
        }

        int instanceId = method.GetCustomAttribute<SpawnAttribute>().InstanceId;
        RegisterSpawn(instanceId, spawn);
      }
    }
  }

  List<NetworkField> FindFields(Type type) {
    var fields = new List<NetworkField>();

    var members = type
      .GetMembers()
      .OrderBy((_) => _.Name)
      .Where(_ => _.IsDefined(typeof(SyncFieldAttribute), false))
      .ToArray();

    foreach (var member in members) {
      var fieldType = GetMemberType(member);

      var fieldGenericType = typeof(NetworkField<,>)
        .MakeGenericType(type, fieldType);

      var fieldAttribute = member
        .GetCustomAttribute<SyncFieldAttribute>(false);

      var field = (NetworkField)Activator
        .CreateInstance(fieldGenericType, member, fieldAttribute);

      var subFieldAttributes = member
        .GetCustomAttributes<NetSyncSubMemberAttribute>(false)
        .ToArray();

      foreach (var attribute in subFieldAttributes) {
        if (!attribute.OverriddenValues)
          attribute.SetValuesFrom(fieldAttribute);

        var subField = FindSubField(type, fieldType, field, attribute); //can throw
        fields.Add(subField);
      }

      if (subFieldAttributes.Length == 0) {
        fields.Add(field);
      }
    }

    return fields;
  }

  NetworkField FindSubField(
      Type type, 
      Type fieldType, 
      NetworkField field, 
      NetSyncSubMemberAttribute attribute
  ) {
    var members = fieldType
      .GetMembers()
      .OrderBy(x => x.Name);

    foreach (var member in members) {
      if (!member.Name.Equals(attribute.MemberName)) continue;

      var subFieldType = GetMemberType(member);

      var subFieldGenericType = typeof(NetworkField<,,>)
        .MakeGenericType(type, fieldType, subFieldType);

      var subField = (NetworkField)Activator
        .CreateInstance(subFieldGenericType, member, field, attribute);

      return subField;
    }
    throw new MissingMemberException(attribute.MemberName);
  }

  Type GetMemberType(MemberInfo member) {
    return member.MemberType == MemberTypes.Field
      ? (member as FieldInfo).FieldType
      : (member as PropertyInfo).PropertyType;
  }

  void RegisterSpawn(int id, SpawnDelegate spawn) {
    spawns.Add(id, spawn);
  }

  public SpawnDelegate GetSpawn(int id) {
    try {
      return spawns[id];
    } catch {
      throw new SpawnNotFoundException(id);
    }
  }

  public NetworkField[] GetFields(ComponentType type) {
    return cashedFields[type];
  }

  public int GetId(ComponentType type) {
    return typeIds[type];
  }

  public ComponentType GetType(int id) {
    return idTypes[id];
  }

  public int GetFieldsCount(Type type) {
    return FieldCounts[type];
  }
}