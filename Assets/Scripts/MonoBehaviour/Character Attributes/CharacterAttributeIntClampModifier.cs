﻿using UnityEngine;
using System.Collections;
using System;

public class CharacterAttributeIntClampModifier : EventTransceiverBehavior, IModifiable<int> {

	//serialized data
	public ModifiableID id;
	public int defaultValue = 0;
	public ModifiableID targetModifiableID = ModifiableID.None;
	public int modifierPriority = 3;
	public bool clampBaseValue = false;
	
	private ISingleOperandModifier<TypedValue32<ModifiableType, int>> modifiers = null;
	private TypedValue32<ModifiableType, int> baseValue = 0;
	private Guid modifierID = Guid.NewGuid();
	private bool startCalled = false;

	#region IModifiable implementation
	public ModifiableID ID {
		get {
			return this.id;
		}
		set {
			this.id = value;
		}
	}
	
	public ISingleOperandModifier<TypedValue32<ModifiableType, int>> Modifiers {
		get {
			if (this.modifiers == null)
				this.modifiers = new GetterModifier<TypedValue32<ModifiableType, int>>();
			return this.modifiers;
		}
	}
	
	public TypedValue32<ModifiableType, int> BaseValue {
		get {
			return baseValue;
		}
		set {
			baseValue = value;
			CallEvent(1, this, this);
		}
	}
	
	public TypedValue32<ModifiableType, int> FinalValue {
		get {
			CallEvent(0, this, this);
			return Modifiers.Resolve(BaseValue);
		}
	}
	#endregion

	public ModifiableID TargetModifiableID {
		get {
			return targetModifiableID;
		}
	}

	// Use this for initialization
	void Start () {
		this.modifiers = new GetterModifier<TypedValue32<ModifiableType, int>>();
		this.BaseValue = defaultValue;	
		startCalled = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	#region implemented abstract members of EventTransceiverBehavior

	public override void ReceiveEvent (string eventName, object args, object sender)
	{
		IModifiable<int> modifiable = args as IModifiable<int>;
		MonoBehaviour senderBehavior = sender as MonoBehaviour;
		if (modifiable != null && senderBehavior != null &&
			modifiable.ID == targetModifiableID && senderBehavior.gameObject.Equals(this.gameObject))
		{
			if (!startCalled)
				Start();
			if (clampBaseValue)
			{
				TypedValue32<ModifiableType, int> clampValue = FinalValue;
				if ((ID == ModifiableID.AttributeClampMaximum && modifiable.BaseValue.Value > clampValue.Value) ||
				    (ID == ModifiableID.AttributeClampMinimum && modifiable.BaseValue.Value < clampValue.Value))
					modifiable.BaseValue = new TypedValue32<ModifiableType, int>(modifiable.BaseValue.Type | clampValue.Type, clampValue.Value);
			}
			else
				modifiable.Modifiers.SetModifier(modifierPriority, modifierID, ClampMaxValue);
		}
	}

	#endregion

	private TypedValue32<ModifiableType, int> ClampMaxValue(TypedValue32<ModifiableType, int> currentValue)
	{
		TypedValue32<ModifiableType, int> clampValue = FinalValue;
		if (ID == ModifiableID.AttributeClampMaximum)
			return currentValue.Value > clampValue.Value ? clampValue : currentValue;
		else if (ID == ModifiableID.AttributeClampMinimum)
			return currentValue.Value < clampValue.Value ? clampValue : currentValue;
		else
			return currentValue;
	}
}
