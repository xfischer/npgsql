/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace pcap2latex.Serialization;

[XmlRoot(ElementName = "field")]
public class Field
{
    [XmlAttribute(AttributeName = "name")]

    public string Name { get; set; }
    [XmlAttribute(AttributeName = "pos")]
    public string Pos { get; set; }
    [XmlAttribute(AttributeName = "show")]
    public string Show { get; set; }
    [XmlAttribute(AttributeName = "showname")]
    public string Showname { get; set; }
    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; }
    [XmlAttribute(AttributeName = "size")]
    public string Size { get; set; }
    [XmlElement(ElementName = "field")]
    public List<Field> Fields { get; set; }
    [XmlAttribute(AttributeName = "hide")]
    public string Hide { get; set; }
    [XmlAttribute(AttributeName = "unmaskedvalue")]
    public string Unmaskedvalue { get; set; }
}

[XmlRoot(ElementName = "proto")]
[DebuggerDisplay("{Name} {Showname} {Fields[0].Show}")]
public class Proto
{
    [XmlElement(ElementName = "field")]
    public List<Field> Fields { get; set; }
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "pos")]
    public string Pos { get; set; }
    [XmlAttribute(AttributeName = "showname")]
    public string Showname { get; set; }
    [XmlAttribute(AttributeName = "size")]
    public string Size { get; set; }

}

[XmlRoot(ElementName = "packet")]
public class Packet
{
    [XmlElement(ElementName = "proto")]
    public List<Proto> Proto { get; set; }
}

[XmlRoot(ElementName = "pdml")]
public class Pdml
{
    [XmlElement(ElementName = "packet")]
    public List<Packet> Packet { get; set; }
    [XmlAttribute(AttributeName = "version")]
    public string Version { get; set; }
    [XmlAttribute(AttributeName = "creator")]
    public string Creator { get; set; }
    [XmlAttribute(AttributeName = "time")]
    public string Time { get; set; }
    [XmlAttribute(AttributeName = "capture_file")]
    public string Capture_file { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.