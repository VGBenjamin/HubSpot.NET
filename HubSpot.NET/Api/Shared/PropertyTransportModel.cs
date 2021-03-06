﻿namespace HubSpot.NET.Api.Shared
{
    using HubSpot.NET.Api.Contact.Dto;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class PropertyTransportModel<T>
    {
        [DataMember(Name = "properties")]
        public List<PropertyValuePair> Properties { get; set; } = new List<PropertyValuePair>();

        
        public void ToPropertyTransportModel(T model)
        {
            PropertyInfo[] properties = model.GetType().GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                var memberAttrib = prop.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute;
                object value = prop.GetValue(model);

                if (value == null || memberAttrib == null)
                {
                    continue;
                }

                if (prop.PropertyType.IsArray && typeof(PropertyValuePair).IsAssignableFrom(prop.PropertyType.GetElementType()))
                {
                    PropertyValuePair[] pairs = value as PropertyValuePair[];
                    Properties.AddRange(pairs);
                    continue;
                }
                else if(typeof(IEnumerable<>).IsAssignableFrom(prop.PropertyType) && typeof(PropertyValuePair).IsAssignableFrom(prop.PropertyType.GetElementType()))
                {
                    IEnumerable<PropertyValuePair> pairs = value as IEnumerable<PropertyValuePair>;
                    Properties.AddRange(pairs);
                    continue;
                }

                Properties.Add(new PropertyValuePair() { Property = memberAttrib.Name, Value = value.ToString() });
            }
        }

        public void ToPropertyTransportModel(ContactHubSpotModel model)
        {
            foreach(KeyValuePair<string, ContactProperty> pair in model.Properties)
            {
                Properties.Add(new PropertyValuePair() { Property = pair.Key, Value = pair.Value.Value });
            }
        }

        public void FromPropertyTransportModel(out T model)
        {
            model = (T) Assembly.GetAssembly(typeof(T)).CreateInstance(typeof(T).FullName);

            PropertyInfo[] props = model.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                var memberAttrib = prop.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute;

                PropertyValuePair pair = Properties.Find(x => x.Property == memberAttrib.Name);
                prop.SetValue(model, pair.Value);
            }
        }
    }
}
