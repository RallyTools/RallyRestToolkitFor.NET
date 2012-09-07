using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections;

namespace RallyJsonToolkit
{
    public class AdhocResponse : DynamicJsonObject
    {
        public AdhocResponse(DynamicJsonObject obj) : base(obj.Dictionary) { }
        public int TotalResultCount { get { return (int)base.Dictionary["TotalResultCount"]; } }
        public IEnumerable<dynamic> Errors { get { return GetCollection(base.Dictionary["Errors"]); } }
        public IEnumerable<dynamic> Warnings { get { return GetCollection(base.Dictionary["Warnings"]); } }
        public IEnumerable<dynamic> Results
        {
            get
            {
                return GetCollection(base.Dictionary["Results"]);
            }
        }
        private IEnumerable<dynamic> GetCollection(object arr)
        {
            ArrayList list = arr as ArrayList;
            foreach (dynamic i in list)
            {
                yield return i;
            }
        }
    }
}
