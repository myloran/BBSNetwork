using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Entities;

public delegate Entity NetworkInstantiationHandlerDelegate(EntityManager entityManager);
internal delegate void RefAction<S, T>(ref S instance, T value);
internal delegate T RefFunc<S, T>(ref S instance);

internal abstract class NetworkField { }

internal abstract class NetworkField<OBJ> : NetworkField {
  public readonly NetSyncBaseAttribute syncAttribute;

  public NetworkField(NetSyncBaseAttribute syncAttribute) {
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
  public readonly RefFunc<OBJ, TYPE> GetValueDelegate;
  public readonly RefAction<OBJ, TYPE> SetValueDelegate;
  //private readonly MemberInfo memberInfo;
  readonly NetworkField parent;
  NetworkMath networkMath;

  public NetworkField(
    MemberInfo info, 
    NetSyncBaseAttribute syncAttribute) : base(syncAttribute
  ) {
    //Debug.Log(typeof(OBJ) + " --- " + typeof(TYPE));
    if (info.MemberType == MemberTypes.Field) {
      var fieldInfo = (FieldInfo)info;
      GetValueDelegate = NetworkMemberInfoUtility.CreateGetter<OBJ, TYPE>(fieldInfo);
      SetValueDelegate = NetworkMemberInfoUtility.CreateSetter<OBJ, TYPE>(fieldInfo);

    } else if (info.MemberType == MemberTypes.Property) {
      var propertyInfo = (PropertyInfo)info;

      GetValueDelegate = (RefFunc<OBJ, TYPE>)Delegate.CreateDelegate(
        typeof(Func<OBJ, TYPE>),
        propertyInfo.GetGetMethod());

      SetValueDelegate = (RefAction<OBJ, TYPE>)Delegate.CreateDelegate(
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
      NetSyncBaseAttribute syncAttribute) : base(syncAttribute
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

internal sealed class NetworkMemberInfo<Parent_OBJ, OBJ, TYPE> : NetworkField<Parent_OBJ> {
  public readonly RefFunc<OBJ, TYPE> GetValueDelegate;
  public readonly RefAction<OBJ, TYPE> SetValueDelegate;
  //private readonly MemberInfo memberInfo;
  readonly NetworkField<Parent_OBJ, OBJ> parent;
  NetworkMath networkMath;

  public NetworkMemberInfo(
      MemberInfo info,
      NetworkField parent,
      NetSyncBaseAttribute syncAttribute) : base(syncAttribute
  ) {
    this.parent = (NetworkField<Parent_OBJ, OBJ>)parent;
    //Debug.Log(typeof(Parent_OBJ) + " --- " + typeof(OBJ) + " --- " + typeof(TYPE));
    if (info.MemberType == MemberTypes.Field) {
      var fieldInfo = (FieldInfo)info;
      GetValueDelegate = NetworkMemberInfoUtility.CreateGetter<OBJ, TYPE>(fieldInfo);
      SetValueDelegate = NetworkMemberInfoUtility.CreateSetter<OBJ, TYPE>(fieldInfo);

    } else if (info.MemberType == MemberTypes.Property) {
      var propertyInfo = (PropertyInfo)info;
      GetValueDelegate = (RefFunc<OBJ, TYPE>)Delegate.CreateDelegate(typeof(RefFunc<OBJ, TYPE>), propertyInfo.GetGetMethod());
      SetValueDelegate = (RefAction<OBJ, TYPE>)Delegate.CreateDelegate(typeof(RefAction<OBJ, TYPE>), propertyInfo.GetSetMethod());

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
      GetValueDelegate(ref obj), 
      oldValue, 
      newValue, 
      deltaTimeFrame, 
      deltaTimeMessage);

    SetValueDelegate(ref obj, type);
    parent.SetValueDelegate(ref parentObj, obj);
  }

  public override int GetValue(Parent_OBJ parentObj) {
    OBJ obj = parent.GetValueDelegate(ref parentObj);
    return networkMath.NativeToInteger(GetValueDelegate(ref obj));
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


internal static class NetworkMemberInfoUtility {
  public static RefFunc<S, T> CreateGetter<S, T>(FieldInfo field) {
    var instance = Expression.Parameter(
      typeof(S).MakeByRefType(), 
      "instance");

    var memberAccess = Expression.MakeMemberAccess(
      instance, 
      field);

    var expr = Expression.Lambda<RefFunc<S, T>>(
      memberAccess, 
      instance);

    return expr.Compile();
  }

  public static RefAction<S, T> CreateSetter<S, T>(FieldInfo field) {
    var instance = Expression.Parameter(
      typeof(S).MakeByRefType(), 
      "instance");

    var value = Expression.Parameter(
      typeof(T), 
      "value");

    var assign = Expression.Assign(
      Expression.Field(instance, field),
      Expression.Convert(value, field.FieldType));

    var expr = Expression.Lambda<RefAction<S, T>>(
      assign, 
      instance, 
      value);

    return expr.Compile();
  }
}

internal class ReflectionUtility {
  public readonly ComponentType[] ComponentTypes;
  Dictionary<ComponentType, NetworkField[]> cashedFields = new Dictionary<ComponentType, NetworkField[]>();
  readonly Dictionary<ComponentType, int> typeIds = new Dictionary<ComponentType, int>();
  readonly Dictionary<int, ComponentType> idTypes = new Dictionary<int, ComponentType>();
  readonly Dictionary<ComponentType, int> FieldCounts = new Dictionary<ComponentType, int>();
  //public static readonly MethodInfo[] NetworkFactoryMethods;
  Dictionary<int, NetworkInstantiationHandlerDelegate> entityFactoryMethodMap = new Dictionary<int, NetworkInstantiationHandlerDelegate>();

  public ReflectionUtility() {
    byte id = 0;
    var componentTypes = new List<ComponentType>();
    //var networkFactoryMethods = new List<MethodInfo>();
    var assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
    assemblies.Sort((x, y) => x.FullName.CompareTo(y.FullName));

    foreach (var assembly in assemblies) {
      var types = new List<Type>(assembly.GetTypes());
      types.Sort((x, y) => x.Name.CompareTo(y.Name));

      foreach (var type in types) {
        if (type.GetCustomAttribute<NetSyncAttribute>() != null) {
          id++;
          typeIds.Add(type, id);
          idTypes.Add(id, type);
          componentTypes.Add(type);

          int fieldsCount = 0;
          var fields = new List<NetworkField>();

          var members = type
            .GetMembers()
            .OrderBy((_) => _.Name)
            .Where(_ => _.IsDefined(typeof(FieldSyncAttribute), false))
            .ToArray();

          foreach (var member in members) {
            var fieldType = typeof(NetworkField<,>);

            var parentType = member.MemberType == MemberTypes.Field
              ? (member as FieldInfo).FieldType
              : (member as PropertyInfo).PropertyType;

            var genericType = fieldType.MakeGenericType(type, parentType);
            var syncAttribute = member.GetCustomAttribute<FieldSyncAttribute>(false);
            var field = (NetworkField)Activator.CreateInstance(genericType, member, syncAttribute);
            var netSyncSubMemberAttributes = member.GetCustomAttributes<NetSyncSubMemberAttribute>(false).ToArray();

            fieldsCount += netSyncSubMemberAttributes.Length;
            foreach (NetSyncSubMemberAttribute NetSyncSubMemberAttribute in netSyncSubMemberAttributes) {
              if (!NetSyncSubMemberAttribute.OverriddenValues) {
                NetSyncSubMemberAttribute.SetValuesFrom(syncAttribute);
              }

              Type subType = member.MemberType == MemberTypes.Field ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType;

              bool found = false;
              IEnumerable<MemberInfo> subMemberInfos = subType.GetMembers().OrderBy(x => x.Name);
              foreach (MemberInfo subMemberInfo in subMemberInfos) {
                if (subMemberInfo.Name.Equals(NetSyncSubMemberAttribute.MemberName)) {
                  Type networkSubMemberInfoType = typeof(NetworkMemberInfo<,,>);
                  Type mainSubMemberInfoTypeType = subMemberInfo.MemberType == MemberTypes.Field ? (subMemberInfo as FieldInfo).FieldType : (subMemberInfo as PropertyInfo).PropertyType;
                  Type subMemberInfoGenericType = networkSubMemberInfoType.MakeGenericType(type, subType, mainSubMemberInfoTypeType);
                  fields.Add((NetworkField)Activator.CreateInstance(subMemberInfoGenericType, subMemberInfo, field, NetSyncSubMemberAttribute));
                  found = true;
                  break;
                }
              }

              if (!found) {
                throw new MissingMemberException(NetSyncSubMemberAttribute.MemberName);
              }
            }

            if (netSyncSubMemberAttributes.Length == 0) {
              fieldsCount++;
              fields.Add(field);
            }
          }
          cashedFields.Add(type, fields.ToArray());
          FieldCounts.Add(type, fieldsCount);
        }

        if (type.GetCustomAttribute<NetworkEntityFactoryAttribute>() != null) {
          //networkFactoryMethods.AddRange(type.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(NetworkInstantiatorAttribute), false)));
          MethodInfo[] methodInfos = type.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(NetworkEntityFactoryMethodAttribute), false)).ToArray();
          foreach (MethodInfo methodInfo in methodInfos) {
            NetworkInstantiationHandlerDelegate networkInstantiationHandlerDelegate = null;
            try {
              networkInstantiationHandlerDelegate = (NetworkInstantiationHandlerDelegate)Delegate.CreateDelegate(typeof(NetworkInstantiationHandlerDelegate), methodInfo);
            } catch (Exception ex) {
              throw new Exception(string.Format("Wrong signature for {0}. Signature requires static Entity {0}(EntityManager)", methodInfo.Name));
            }
            RegisterEntityFactoryMethod(methodInfo.GetCustomAttribute<NetworkEntityFactoryMethodAttribute>().InstanceId, networkInstantiationHandlerDelegate);
          }
        }
      }
    }

    ComponentTypes = componentTypes.ToArray();
    //NetworkFactoryMethods = networkFactoryMethods.ToArray();
  }


  public ReflectionUtility(Assembly assembly) {

    byte componentTypeId = 0;
    var componentTypes = new List<ComponentType>();
    List<Type> types = new List<Type>(assembly.GetTypes());
    types.Sort((x, y) => x.Name.CompareTo(y.Name));
    foreach (Type type in types) {
      if (type.GetCustomAttribute<NetSyncAttribute>() != null) {
        componentTypeId++;
        typeIds.Add(type, componentTypeId);
        idTypes.Add(componentTypeId, type);
        componentTypes.Add(type);

        int numberOfMembers = 0;
        List<NetworkField> networkMemberInfos = new List<NetworkField>();
        MemberInfo[] memberInfos = type.GetMembers().OrderBy((x) => x.Name).Where(memberInfo => memberInfo.IsDefined(typeof(FieldSyncAttribute), false)).ToArray();
        for (int i = 0; i < memberInfos.Length; i++) {
          MemberInfo memberInfo = memberInfos[i];

          Type networkMemberInfoType = typeof(NetworkField<,>);
          Type mainMemberInfoTypeType = memberInfo.MemberType == MemberTypes.Field ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
          Type mainMemberInfoGenericType = networkMemberInfoType.MakeGenericType(type, mainMemberInfoTypeType);
          FieldSyncAttribute netSyncMemberAttribute = memberInfo.GetCustomAttribute<FieldSyncAttribute>(false);
          NetworkField mainMemberInfo = (NetworkField)Activator.CreateInstance(mainMemberInfoGenericType, memberInfo, netSyncMemberAttribute);
          NetSyncSubMemberAttribute[] netSyncSubMemberAttributes = memberInfo.GetCustomAttributes<NetSyncSubMemberAttribute>(false).ToArray();

          numberOfMembers += netSyncSubMemberAttributes.Length;
          foreach (NetSyncSubMemberAttribute NetSyncSubMemberAttribute in netSyncSubMemberAttributes) {
            if (!NetSyncSubMemberAttribute.OverriddenValues) {
              NetSyncSubMemberAttribute.SetValuesFrom(netSyncMemberAttribute);
            }

            Type subType = memberInfo.MemberType == MemberTypes.Field ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;

            bool found = false;
            IEnumerable<MemberInfo> subMemberInfos = subType.GetMembers().OrderBy(x => x.Name);
            foreach (MemberInfo subMemberInfo in subMemberInfos) {
              if (subMemberInfo.Name.Equals(NetSyncSubMemberAttribute.MemberName)) {
                Type networkSubMemberInfoType = typeof(NetworkMemberInfo<,,>);
                Type mainSubMemberInfoTypeType = subMemberInfo.MemberType == MemberTypes.Field ? (subMemberInfo as FieldInfo).FieldType : (subMemberInfo as PropertyInfo).PropertyType;
                Type subMemberInfoGenericType = networkSubMemberInfoType.MakeGenericType(type, subType, mainSubMemberInfoTypeType);
                networkMemberInfos.Add((NetworkField)Activator.CreateInstance(subMemberInfoGenericType, subMemberInfo, mainMemberInfo, NetSyncSubMemberAttribute));
                found = true;
                break;
              }
            }

            if (!found) {
              throw new MissingMemberException(NetSyncSubMemberAttribute.MemberName);
            }
          }

          if (netSyncSubMemberAttributes.Length == 0) {
            numberOfMembers++;
            networkMemberInfos.Add(mainMemberInfo);
          }
        }
        cashedFields.Add(type, networkMemberInfos.ToArray());
        FieldCounts.Add(type, numberOfMembers);
      }

      if (type.GetCustomAttribute<NetworkEntityFactoryAttribute>() != null) {
        MethodInfo[] methodInfos = type.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(NetworkEntityFactoryMethodAttribute), false)).ToArray();
        foreach (MethodInfo methodInfo in methodInfos) {
          NetworkInstantiationHandlerDelegate networkInstantiationHandlerDelegate = null;
          try {
            networkInstantiationHandlerDelegate = (NetworkInstantiationHandlerDelegate)Delegate.CreateDelegate(typeof(NetworkInstantiationHandlerDelegate), methodInfo);
          } catch (Exception ex) {
            throw new Exception(string.Format("Wrong signature for {0}. Signature requires static Entity {0}(EntityManager)", methodInfo.Name));
          }
          RegisterEntityFactoryMethod(methodInfo.GetCustomAttribute<NetworkEntityFactoryMethodAttribute>().InstanceId, networkInstantiationHandlerDelegate);
        }
      }
    }

    ComponentTypes = componentTypes.ToArray();
    //NetworkFactoryMethods = networkFactoryMethods.ToArray();
  }

  public void RegisterEntityFactoryMethod(int id, NetworkInstantiationHandlerDelegate networkInstantiationHandler) {
    entityFactoryMethodMap.Add(id, networkInstantiationHandler);
  }

  public NetworkInstantiationHandlerDelegate GetEntityFactoryMethod(int id) {
    try {
      return entityFactoryMethodMap[id];
    } catch {
      throw new NetworkEntityFactoryMethodNotFoundException(id);
    }
  }

  public NetworkField[] GetNetworkMemberInfo(ComponentType componentType) {
    return cashedFields[componentType];
  }

  public int GetComponentTypeID(ComponentType componentType) {
    return typeIds[componentType];
  }

  public ComponentType GetComponentType(int id) {
    return idTypes[id];
  }

  public int GetNumberOfMembers(Type componentType) {
    return FieldCounts[componentType];
  }
}