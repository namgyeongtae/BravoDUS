using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Bind : Attribute
{
    public string ObjectName = string.Empty;

    public Bind(string objectName)
    {
        ObjectName = objectName;
    }
}

public class BindAttribute
{
    private class BindInfo
    {
        public readonly FieldInfo FieldInfo;
        public readonly Bind Attribute;

        public BindInfo(FieldInfo fieldInfo, Bind attribute)
        {
            FieldInfo = fieldInfo;
            Attribute = attribute;
        }
    }

    private static BindInfo[] _container = null;

    public static void InstallBindings(MonoBehaviour target)
    {
        _container = target.GetType()
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => p.IsDefined(typeof(Bind), false))
                        .Select(p => new BindInfo(p, GetBindAttributes(p)))
                        .ToArray();

        if (_container == null)
            return;
        
        // UI의 자식 오브젝트들을 이름을 Key값으로 캐싱할 Dictionary
        Dictionary<string, Transform> cachedChildTransform = new Dictionary<string, Transform>();

        target.transform.CachedChildTransform(cachedChildTransform);

        foreach (var item in _container)
        {
            if (cachedChildTransform.TryGetValue(item.Attribute.ObjectName, out var outTransform))
            {
                if (item.FieldInfo.FieldType == typeof(GameObject))
                {
                    item.FieldInfo.SetValue(target, outTransform.gameObject);
                }
                else
                {
                    // 배열 필드일 경우
                    // ex) [Bind("Buttons")] private Button[] _buttons; 의 경우
                    // Buttons라는 이름의 자식오브젝트들 중 Button 컴포넌트를 가진 오브젝트들을 배열로 반환
                    if (item.FieldInfo.FieldType.IsArray)
                    {
                        Type type = item.FieldInfo.FieldType.GetElementType();
                        Component[] components = outTransform.GetComponentsInChildren(type, true);
                        Array filledArray = Array.CreateInstance(type, components.Length);
                        Array.Copy(components, filledArray, components.Length);
                        item.FieldInfo.SetValue(target, filledArray);
                    }
                    else
                    {
                        var component = outTransform.GetComponent(item.FieldInfo.FieldType);
                        if (component == null)
                            continue;
                        item.FieldInfo.SetValue(target, component);
                    }
                }
                if (outTransform.TryGetComponent<UIBindBase>(out var bindBase))
                {
                    // BindBase 오브젝트 하위에 있는 또 다른 BindBase 먼저 Binding
                    bindBase.InstallBindings();
                }
            }
        }

        cachedChildTransform.Clear();
    }

    private static Bind GetBindAttributes(FieldInfo fieldInfo)
    {
        return fieldInfo.GetCustomAttributes(typeof(Bind), false).FirstOrDefault() as Bind;
    }
}
