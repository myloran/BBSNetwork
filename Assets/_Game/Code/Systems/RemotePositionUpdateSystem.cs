﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class RemotePositionUpdateSystem : ComponentSystem {
  struct Data {
    public ComponentArray<Transform> transforms;
    public ComponentDataArray<Position> positions;
    public ComponentDataArray<Heading> headings;
    public SubtractiveComponent<Velocity> velocities;
    public readonly int Length;
  }
  [Inject] Data data;

  protected override void OnUpdate() {
    for (int i = 0; i < data.Length; i++) {
      var transform = data.transforms[i];
      transform.position = data.positions[i].Value;
      var heading = data.headings[i].Value;
      if (heading != Vector3.zero)
        transform.rotation = Quaternion.LookRotation(heading);
    }
  }
}