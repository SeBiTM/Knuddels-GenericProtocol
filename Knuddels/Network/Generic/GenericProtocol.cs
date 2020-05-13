using  Knuddels.Network.Generic.DataStream;
using  Knuddels.Network.Generic.IStream;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace  Knuddels.Network.Generic
{
    public class GenericProtocol : IEnumerable, IDisposable
    {
        #region " FELDER "

        //++ PRIVATE
        //+ --------------------------------------------

        /// <summary>
        ///     Gibt das Trennzeichen der Knotennamen im Baumstring zurück.
        /// </summary>
        private const char DELIMITER = ';';

        public byte[] ByteData; // Applet: h // Keine Ahnung wozu dieses ByteArray dient.

        //+ --------------------------------------------
        /* Der Baumstring */
        //+ --------------------------------------------
        /// <summary>
        ///     Gibt die letzte Position im Baumstring zurück.
        /// </summary>
        private int _lastIndex; // Applet: b

        /// <summary>
        ///     Diese Liste beinhaltet alle Positionen der Knoten von einem Knoten.
        /// </summary>
        private List<List<int>> _nodeIndices; // Applet: f

        //+ --------------------------------------------
        /// <summary>
        ///     Gibt eine Auflistung aller Knotennamen zurück.
        /// </summary>
        private List<string> _nodeNames; // Applet: e

        //+ --------------------------------------------
        /// <summary>
        ///     Beinhaltet eine Liste aller NodeNames(als Key). Als Value ist entweder der NodeIndex angegeben oder noch eine
        ///     Dictionary..
        /// </summary>
        private Dictionary<string, object> _nodeValues; // Applet: g

        //+ --------------------------------------------

        //+ --------------------------------------------
        /// <summary>
        ///     Gibt eine Auflistung aller hinzugefügten Nodes zurück./ausgelesener Nodes aus dem Stream zurück.
        /// </summary>
        private Dictionary<string, object> _nodes; // Applet: j

        private string _tree; // Applet: a
        private bool _disposed;
        private readonly object _updateTreeLock = new object();
        //+ --------------------------------------------
        //++ PUBLIC
        //+ --------------------------------------------

        #endregion

        #region " EIGENSCHAFTEN "

        //+ --------------------------------------------
        /// <summary>
        ///     Gibt den Index des Hauptknotens zurück.
        /// </summary>
        public int Index { get; private set; } // Applet: i
        //+ --------------------------------------------
        /// <summary>
        ///     Gibt den Hash des Protokolls zurück. Dient zum vergleichen zweier Baumzeichenketten.
        /// </summary>
        public string Hash { get; private set; } // Applet: d
        //+ --------------------------------------------
        /// <summary>
        ///     Gibt den Namen des Hauptknotens zurück.
        /// </summary>
        public string Name
        {
            get { return NodeNames[Index]; }
        } // Applet: c
        //+ -------------------------------------------------
        /// <summary>
        ///     Gibt eine Auflistung aller Knotennamen zurück.
        /// </summary>
        public List<string> NodeNames
        {
            get { return _nodeNames; }
        }

        //+ --------------------------------------------
        /// <summary>
        ///     Gibt eine Auflistung aller hinzugefügten Nodes zurück./ausgelesener Nodes aus dem Stream zurück.
        /// </summary>
        public Dictionary<string, object> Nodes
        {
            get { return _nodes; }
        }

        //+ --------------------------------------------
        /// <summary>
        ///     Diese Liste beinhaltet alle Positionen der Knoten von einem Knoten.
        /// </summary>
        public List<List<int>> NodeIndices
        {
            get { return _nodeIndices; }
        }

        //+ --------------------------------------------
        /// <summary>
        ///     Beinhaltet eine Liste aller NodeNames(als Key). Als Value ist entweder der NodeIndex angegeben oder noch eine
        ///     Dictionary..
        /// </summary>
        public Dictionary<string, object> NodeValues
        {
            get { return _nodeValues; }
        }

        //+ --------------------------------------------

        #endregion

        #region " KONSTRUKTOR "

        //+ --------------------------------------------

        //+ --------------------------------------------
        private GenericProtocol()
        {
            ByteData = null;
        }

        //+ --------------------------------------------
        private GenericProtocol(int pIndex)
        {
            ByteData = null;
            Index = pIndex;
            _nodes = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Erstellt eine neue Node-Klasse mit dem angegebenen Baum.
        /// </summary>
        /// <param name="pTree">Der Baumstring.</param>
        /// <returns>Die Node-Klasse.</returns>
        public static implicit operator GenericProtocol(string pTree)
        {
            var node = new GenericProtocol();
            node.UpdateTree(pTree);
            return node;
        }

        //+ --------------------------------------------
        ~GenericProtocol()
        {
            Dispose(false);
        }

        #endregion

        #region " COPYREF "

        /// <summary>
        ///     Kopiert die Referenzen(Zeiger) von den Objekten in der Klasse.
        /// </summary>
        /// <returns></returns>
        public GenericProtocol CopyRef()
        {
            return CopyRef(Index);
        }

        /// <summary>
        ///     Kopiert die Referenzen(Zeiger) von den Objekten in der Klasse.
        /// </summary>
        /// <returns></returns>
        public GenericProtocol CopyRef(string pNodeName)
        {
            if (!NodeValues.ContainsKey(pNodeName))
                return null;
            return CopyRef((int)NodeValues[pNodeName]);
        }

        /// <summary>
        ///     Kopiert die Referenzen(Zeiger) von den Objekten in der Klasse.
        /// </summary>
        /// <returns></returns>
        public GenericProtocol CopyRef(int pNodeIndex)
        {
            var node = new GenericProtocol(pNodeIndex)
            {
                _nodeNames = NodeNames,
                _nodeIndices = NodeIndices,
                _nodeValues = NodeValues,
                ByteData = ByteData,
                Hash = Hash,
            };
            return node;
        }

        #endregion

        #region " INDEXER "

        /// <summary>
        ///     Gibt ein Node von der Node-List zurück.
        /// </summary>
        /// <param name="pNodeName"></param>
        /// <returns></returns>
        public GenericProtocol this[string pNodeName]
        {
            get { return Get<GenericProtocol>(pNodeName); }
        }

        #endregion

        #region " NODES "

        /// <summary>
        ///     Fügt ein neues Element in die Node-List hinzu.
        /// </summary>
        /// <param name="pKey">Der Key.</param>
        /// <param name="pObject">Das Objekt.</param>
        public GenericProtocol Add(string pKey, object pObject)
        {
            if (_nodes.ContainsKey(pKey))
                _nodes.Remove(pKey);
            _nodes.Add(pKey, pObject);
            return this;
        }

        /// <summary>
        ///     Gibt ein Objekt von der Node-List zurück.
        /// </summary>
        /// <typeparam name="T">Der Typ, dass das Objekt hat.</typeparam>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public T Get<T>(string pKey)
        {
            if (!_nodes.ContainsKey(pKey))
                return default(T);
            return (T)_nodes[pKey];
        }

        /// <summary>
        ///     Überprüft ob der angegebene Knoten vorhanden ist.
        /// </summary>
        /// <param name="pNodeName">Der zu überprüfende Knotenname.</param>
        /// <returns></returns>
        public bool ContainesNode(string pNodeName)
        {
            return _nodes.ContainsKey(pNodeName);
        }

        /// <summary>
        ///     Gibt die festen Werte eines Knotens zurück. Falls vorhanden.
        /// </summary>
        /// <param name="pNodeName">Der Knotenname.</param>
        /// <param name="pKeyName">Der Wertname.</param>
        /// <returns></returns>
        public int GetValue(string pNodeName, string pKeyName)
        {
            if (!NodeValues.ContainsKey(pNodeName))
                return -1;
            var dic = NodeValues[pNodeName] as Dictionary<string, int>;
            if (dic == null || !dic.ContainsKey(pKeyName))
                return -1;
            return dic[pKeyName];
        }

        #endregion

        #region " READ|WRITE "

        #region " READ "

        /// <summary>
        ///     Methode zum auslesen vom :-Token.
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pStartPosition"></param>
        /// <returns></returns>
        public GenericProtocol Read(string pString, int pStartPosition)
        {
            return Read(new DataInput(pString, pStartPosition));
        }

        /// <summary>
        ///     Zum auslesen direkt aus dem Stream.
        /// </summary>
        /// <param name="pByteData"></param>
        /// <returns></returns>
        public GenericProtocol Read(byte[] pByteData)
        {
            return Read(new DataInputStream(pByteData));
        }

        private GenericProtocol Read(IStreamInput pDataInput)
        {
            var index = pDataInput.ReadShort();
            var node = CopyRef(index);
            // Rekursive Methode:
            Read(pDataInput, index, node);
            return node;
        }

        private object Read(IStreamInput pDataInput, int pIndex, GenericProtocol pNode)
        {
            if (pNode == null)
                pNode = CopyRef(pIndex);
            // --------------------------
            List<int> nodeIndices = pNode.NodeIndices[pIndex];
            for (int i = 0; i < nodeIndices.Count; i++)
            {
                int localNodeIndex = nodeIndices[i];
                string localNodeName;
                switch (localNodeIndex)
                {
                    case 0:
                        return pDataInput.ReadByte(); // Byte
                    case 1:
                        return pDataInput.ReadBoolean(); // Bool(ean)
                    case 2:
                        return pDataInput.ReadByte(); // Byte
                    case 3:
                        return pDataInput.ReadShort(); // Short
                    case 4:
                        return pDataInput.ReadInt(); // Int
                    case 5:
                        return pDataInput.ReadLong(); // Long
                    case 6:
                        return pDataInput.ReadFloat(); // Float
                    case 7:
                        return pDataInput.ReadDouble(); // Double
                    case 8:
                        return pDataInput.ReadChar(); // Char
                    case 9:
                        return pDataInput.ReadUTF().Replace('\u20AD', 'K'); // String
                    case 10:
                        break;
                    case 11:
                        i++;
                        localNodeIndex = nodeIndices[i];
                        localNodeName = NodeNames[localNodeIndex];
                        var arrList = new List<object>();
                        pNode.Add(localNodeName, arrList);
                        while (pDataInput.ReadByte() == 11)
                        {
                            arrList.Add(Read(pDataInput, localNodeIndex, null));
                        }
                        i++;
                        break;
                    case 12: // Ende der Liste
                        break;
                    case 13: // new | seit ungefähr dem applet 90aeh
                        return ReadChars(pDataInput);
                    //break;
                    default:
                        localNodeName = NodeNames[localNodeIndex];
                        pNode.Add(localNodeName, Read(pDataInput, localNodeIndex, null));
                        break;
                }
            }
            // --------------------------
            return pNode;
        }

        #endregion

        #region " READCHARS|WRITECHARS "

        private static string ReadChars(IStreamInput pInput)
        {
            // Länge der Zeichenkette:
            int length = pInput.ReadUnsignedByte(); // b = index 2

            if (length == 255)
                return null;

            if (length >= 128) // offset 52                  
                length = length - 128 << 16 | pInput.ReadUnsignedByte() << 8 | pInput.ReadUnsignedByte();
            // offset 17
            var stringBuilder = new StringBuilder(length + 2); // sb = index 3
            for (int i4 = 0; i4 < length; i4++) // offset 86                
                stringBuilder.Append(pInput.ReadChar());
            // offset 81
            return stringBuilder.ToString();
        }

        private static void WriteChars(string pString, IStreamOutput pOutput)
        {
            if (pString == null)
            {
                pOutput.WriteInt(255);
                return;
            }

            int length = pString.Length;
            // pString.Length == index 3
            if (length >= 128)
            {
                // offset 7
                pOutput.WriteByte(length >> 16 | 0x80);
                pOutput.WriteByte(length >> 8 & 0xff);
                pOutput.WriteByte(length & 0xff);
            }
            //offset 16
            pOutput.WriteByte(length);

            if (length > 0)
                pOutput.WriteChars(pString);
        }

        #endregion

        #region " WRITE "

        public string GetString(GenericProtocol pNode)
        {
            var dataOutput = new DataOutput();
            Write(pNode, pNode.Index, dataOutput);
            return dataOutput.Data;
        }

        public byte[] GetBytes(GenericProtocol pNode)
        {
            using (var dataOutput = new DataOutputStream())
            {
                Write(pNode, pNode.Index, dataOutput);
                return dataOutput.ToArray();
            }
        }

        private void Write(GenericProtocol pNode, int pIndex, IStreamOutput pOutput)
        {
            if (ByteData != null)
                pOutput.Write(ByteData);
            pOutput.WriteShort((short)pIndex);
            // Rekursive Methode:
            Write((object)pNode, pIndex, pOutput);
        }

        private void Write(Object pObject, int pIndex, IStreamOutput pOutput)
        {
            List<int> localNodeIndices = _nodeIndices[pIndex];
            for (int i = 0; i < localNodeIndices.Count; i++)
            {
                int i6 = localNodeIndices[i];
                
                switch (i6)
                {
                    case 0:
                        pOutput.WriteByte((byte)pObject); // Byte
                        break;
                    case 1:
                        pOutput.WriteBoolean((bool)pObject);
                        break;
                    case 2:
                        pOutput.WriteByte((byte)pObject);
                        break;
                    case 3:
                        pOutput.WriteShort((short)pObject);
                        break;
                    case 4:
                        pOutput.WriteInt((int)pObject);
                        break;
                    case 5:
                        pOutput.WriteLong((long)pObject);
                        break;
                    case 6:
                        pOutput.WriteFloat((float)pObject);
                        break;
                    case 7:
                        pOutput.WriteDouble((double)pObject);
                        break;
                    case 8:
                        pOutput.WriteChar((char)pObject);
                        break;
                    case 9:
                        pOutput.WriteUTF((string)pObject);
                        break;
                    case 10:
                        break;
                    case 11:
                        i++;
                        i6 = localNodeIndices[i];
                        string localNodeName = _nodeNames[i6];
                        var listOfObjects = ((GenericProtocol)pObject).Get<List<object>>(localNodeName);
                        if (listOfObjects == null)
                        {
                            pOutput.WriteByte(12);
                            i++;
                            break;
                        }
                        foreach (var t in listOfObjects)
                        {
                            pOutput.WriteByte(11);
                            Write(t, i6, pOutput);
                        }
                        pOutput.WriteByte(12);
                        i++;
                        break;
                    case 12: // Ende der Liste
                        break;
                    case 13:
                        WriteChars((string)pObject, pOutput);
                        break;
                    default:
                        Write(((GenericProtocol)pObject).Get<object>(_nodeNames[i6]), i6, pOutput);
                        break;
                }
            }
        }

        #endregion

        #endregion

        #region " BAUMSTRING - METHODEN "

        /// <summary>
        ///     Aktualisiert den Baumstring.
        /// </summary>
        /// <param name="pTree">Der Baumstring.</param>
        public void UpdateTree(string pTree)
        {
            Debug.WriteLine(pTree);
            lock (_updateTreeLock)
            {
                Reset(pTree); // Listen neu init. und Positionszeiger des Baumstrings auf 0(Anfang) setzen.
                //+ --------------------------------
                Hash = Next<string>(); // Hash auslesen
                Index = Next<int>(); // Hauptknotenindex auslesen
                //+ --------------------------------
                // Füge X NULL-Werte in die Listen hinzu. 'X' steht hierbei für den Wert der 'Index'-Variable. In 90aeq und kleiner sind es 20 (gewesen).
                // Die NULL-Werte sind nicht Sinnlos, sondern dienen für bestimmte Befehle.
                // Hier eine Auflistung der Befehle in 90aeq:
                // 0 = Byte auslesen 
                // 1 = Boolean auslesen 
                // 2 = Byte auslesen 
                // 3 = Short auslesen 
                // 4 = int auslesen
                // 5 = long auslesen 
                // 6 = Float auslesen 
                // 7 = Double auslesen 
                // 8 = Char auslesen 
                // 9 = UTF-String auslesen
                // 11 = Neue Liste 
                // 12 = Ende der Liste 
                // 13 = Verschlüsselter String?
                // Alles über oder gleich dem Wert der 'Index'-Variable = Node
                for (int i = 0; i < Index; i++)
                {
                    NodeNames.Add(null);
                    NodeIndices.Add(null);
                }
                //+ --------------------------------
                // Füge nun alle Knotennamen in die Liste hinzu.
                while (!IsEmpty())
                {
                    NodeNames.Add(Next<string>());
                }
                //+ --------------------------------
                // Manche Knoten haben nicht wie andere Werte oder andere Knoten gespeichert sondern bestimmte Werte.            
                for (int i = Index; i < NodeNames.Count; i++)
                {
                    if (!NodeValues.ContainsKey(NodeNames[i]))
                        NodeValues.Add(NodeNames[i], i);
                }
                //+ --------------------------------
                // Fügt alle Indices eines Knotens in eine Liste hinzu.
                // Key = Knotenname
                // Value = Index
                // Die Indices geben die Unterknoten des Knotens zurück.

                #region " BEISPIEL "

                // Baum: 1337;20;PROTOCOL_HASH;;5;;:
                // KEY = PROTOCOL_HASH
                // VALUE = List<int>() { 5 };
                // 5 Gibt an das der Knoten ein Long zurückgibt. Siehe oben in der Befehlsliste.

                #endregion

                while (NodeIndices.Count < NodeNames.Count)
                {
                    var indices = new List<int>();
                    while (!IsEmpty())
                    {
                        indices.Add(Next<int>());
                    }
                    NodeIndices.Add(indices);
                }
                //+ --------------------------------
                // Springt zur Stelle an der ':' steht.
                Next<object>(':');
                //+ --------------------------------
                for (int i = Index; i < NodeIndices.Count; i++)
                {
                    List<int> indices = NodeIndices[i];
                    foreach (int t in indices)
                    {
                        if (t != 0) continue;
                        var values = new Dictionary<string, int>();
                        for (int k = 0; !IsEmpty(); k++)
                            values.Add(Next<string>(), k);
                        NodeValues[NodeNames[i]] = values;
                    }
                }
            }
        }

        #region " AUSLESEN VOM BAUMSTRING "

        /// <summary>
        ///     Holt den nächsten Teilstring der Zeichenkette.
        /// </summary>
        /// <typeparam name="T">In welchen Typ soll es konvertiert werden.</typeparam>
        /// <returns>Gibt den Teilstring zurück vom angegeben Typen.</returns>
        private T Next<T>()
        {
            return Next<T>(DELIMITER);
        }

        /// <summary>
        ///     Holt den nächsten Teilstring der Zeichenkette.
        /// </summary>
        /// <typeparam name="T">In welchen Typ soll es konvertiert werden.</typeparam>
        /// <param name="paramDelimiter"></param>
        /// <returns>Gibt den Teilstring zurück vom angegeben Typen.</returns>
        private T Next<T>(char paramDelimiter)
        {
            int nextIndex = _tree.IndexOf(paramDelimiter, _lastIndex);
            if (nextIndex >= 0)
            {
                string strNext = _tree.Substring(_lastIndex, nextIndex - _lastIndex);
                _lastIndex = nextIndex + 1;
                return (T)Convert.ChangeType(strNext, typeof(T));
            }
            return default(T);
        }

        /// <summary>
        ///     Überprüft ob der nächste Teilstring leer ist. Wenn True dann wird die letzte Position um eine Stelle addiert.
        /// </summary>
        /// <returns>Wenn Leer dann wird True zurückgegeben.</returns>
        private bool IsEmpty()
        {
            var ind = _tree.IndexOf(DELIMITER, _lastIndex);
            if (ind == -1)
                throw new Exception("Failed to parse GenericTree");
            if (ind == _lastIndex)
            {
                _lastIndex = _tree.IndexOf(DELIMITER, _lastIndex) + 1;
                return true;
            }
            return false;
        }

        #endregion

        #region Reset

        /// <summary>
        ///     Setzt die Position wieder an Anfang(=0) und initialisiert die Objekte neu.
        /// </summary>
        private void Reset()
        {
            _nodeNames = new List<string>();
            _nodeIndices = new List<List<int>>();
            _nodeValues = new Dictionary<string, object>();
            _nodes = new Dictionary<string, object>();
            _lastIndex = 0;
        }

        /// <summary>
        ///     Setzt die Position wieder an Anfang(=0) und übergibt einen neuen Protocol-Baum.
        /// </summary>
        /// <param name="paramTree"></param>
        private void Reset(string paramTree)
        {
            Reset();
            _tree = paramTree;
        }

        #endregion

        #endregion

        #region " TO "

        #region " TOSTRING "

        /// <summary>
        ///     Gibt den Namen des Hauptknotens zurück.
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            return ToString(true);
        }
        public string ToString(bool withElements)
        {
            return ToString(this);
        }
        private string ToString(GenericProtocol node, int index = 0)
        {
            var append = string.Empty;
            for (var i = 0; i < index; i++)
                append += "\t";

            var builder = new StringBuilder();
            builder.Append(append).Append(node.Name).Append(" (GenericProtocol)\n");
            builder.Append(append).Append("{\n");
            foreach (var pair in node._nodes)
            {
                var type = pair.Value == null ? "NULL" : pair.Value.GetType().Name;
                if (pair.Value != null && pair.Value.GetType() == typeof(List<object>))
                {
                    if (((List<object>)pair.Value).Count > 0)
                        type = string.Format("List<{0}>", ((List<object>)pair.Value)[0].GetType().Name);
                }
                builder.Append(append).Append("\t").Append(pair.Key).Append(" (").Append(type).Append(")\n");
                
                if (pair.Value != null && pair.Value.GetType() == typeof(GenericProtocol))
                {
                    builder.Append(ToString((GenericProtocol)pair.Value, index + 1));
                } else if (pair.Value != null && pair.Value.GetType() == typeof(List<object>))
                {
                    builder.Append(append).Append("\t{\n");

                    foreach (var val in (List<object>)pair.Value) {
                        if (val.GetType() == typeof(GenericProtocol)) {
                            builder.Append(append).Append(ToString((GenericProtocol)val, index + 2)).Append("\n");
                        } else
                        {
                            builder.Append(append).Append("\t\t").Append(val).Append("\n");
                        }
                    }

                    builder.Append(append).Append("\t}\n");
                }
                else
                {
                    builder.Append(append).Append("\t\t").Append(pair.Value).Append("\n");
                }

            }
            builder.Append(append).Append("}\n");

            return builder.ToString();
        }
        #endregion

        #region " TOTREE "

        /// <summary>
        ///     Wandelt die Klasse in ein Baumstring.
        /// </summary>
        /// <returns></returns>
        public string ToTree()
        {
            var stringBuilder = new StringBuilder();
            // ---------------------------
            stringBuilder.Append(Hash);
            stringBuilder.Append(DELIMITER);
            stringBuilder.Append(Index);
            stringBuilder.Append(DELIMITER);
            // ---------------------------
            foreach (string nodeName in NodeNames)
            {
                if (nodeName == null)
                    continue;
                stringBuilder.Append(nodeName).Append(DELIMITER);
            }
            // ---------------------------
            foreach (var listOfInt in NodeIndices)
            {
                if (listOfInt == null)
                    continue;
                stringBuilder.Append(DELIMITER);
                foreach (int i in listOfInt)
                {
                    stringBuilder.Append(i).Append(DELIMITER);
                }
            }
            // ---------------------------
            stringBuilder.Append(DELIMITER);
            stringBuilder.Append(":");
            // ---------------------------
            foreach (object obj in NodeValues.Values)
            {
                Dictionary<string, int> ints = obj as Dictionary<string, int>;
                if (ints != null)
                {
                    var nodeValue = ints;
                    foreach (string key in nodeValue.Keys)
                    {
                        stringBuilder.Append(key).Append(DELIMITER);
                    }
                    stringBuilder.Append(DELIMITER);
                }
            }
            // ---------------------------
            stringBuilder.Append(DELIMITER);
            // ---------------------------
            return stringBuilder.ToString();
        }

        #endregion

        #region " TOSTRINGDICTIONARY "

        //++ INFO
        // Der Knoten 'KEY_VALUE' enthält eine Liste mit Schlüsseln und Werten.
        // Diese Schlüssel und Werte kann man in eine Hashtable(Java) oder in eine Dictionary(.Net) umwandeln.
        // Oder anders rum.
        //+ --------------------

        /// <summary>
        ///     Wandelt die Liste mit dem 'KEY_VALUE'-Knoten in eine Dictionary um.
        /// </summary>
        /// <param name="pKeyValueList"></param>
        /// <returns></returns>
        public Dictionary<string, string> ToStringDictionary(List<object> pKeyValueList)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (object t in pKeyValueList)
            {
                var keyValue = t as GenericProtocol;
                if (keyValue != null) dic.Add(keyValue.Get<string>("KEY"), keyValue.Get<string>("VALUE"));
            }
            return dic;
        }

        /// <summary>
        ///     Wandelt eine StringDictionary in eine Liste mit 'KEY_VALUE'-Knoten um.
        /// </summary>
        /// <param name="pStringDictionary"></param>
        /// <returns></returns>
        public List<object> ToNodeFromStringDictionary(Dictionary<string, string> pStringDictionary)
        {
            var keyValueList = new List<object>();
            foreach (var keyValue in pStringDictionary)
            {
                GenericProtocol copyRef = CopyRef("KEY_VALUE");
                if (copyRef == null)
                    continue;
                copyRef.Add("KEY", keyValue.Key);
                copyRef.Add("VALUE", keyValue.Value);
                keyValueList.Add(copyRef);
            }
            return keyValueList;
        }

        #endregion

        #endregion

        #region " YFIELD "

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in Nodes)
                yield return item;
        }

        #endregion

        #region " EQUALS "

        public override bool Equals(object pObject)
        {
            if (pObject.GetType() != typeof(GenericProtocol))
                return false;
            // --------------------------
            var node = (GenericProtocol)pObject;
            // --------------------------
            return Hash.Equals(node.Hash);
        }

        #endregion

        #region " SONSTIGES "

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_tree != null ? _tree.GetHashCode() : 0);
                result = (result * 397) ^ _lastIndex;
                result = (result * 397) ^ (_nodeNames != null ? _nodeNames.GetHashCode() : 0);
                result = (result * 397) ^ (_nodeValues != null ? _nodeValues.GetHashCode() : 0);
                result = (result * 397) ^ (_nodeIndices != null ? _nodeIndices.GetHashCode() : 0);
                result = (result * 397) ^ (_nodes != null ? _nodes.GetHashCode() : 0);
                result = (result * 397) ^ (ByteData != null ? ByteData.GetHashCode() : 0);
                result = (result * 397) ^ Index;
                result = (result * 397) ^ (Hash != null ? Hash.GetHashCode() : 0);
                return result;
            }
        }

        #endregion

        #region " DISPOSE "

        /// <summary>
        ///     Gibt alle Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool pDisposing)
        {
            if (!_disposed && pDisposing)
            {
                _tree = null;
                _lastIndex = 0;
                if (_nodeNames != null)
                    _nodeNames.Clear();
                _nodeNames = null;
                if (_nodeIndices != null)
                    _nodeIndices.Clear();
                _nodeIndices = null;
                if (_nodes != null)
                    _nodes.Clear();
                _nodes = null;
                if (_nodeValues != null)
                    _nodeValues.Clear();
                _nodeValues = null;
                ByteData = null;
            }
            _disposed = true;
        }

        #endregion
    }
}