//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjectX.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Options
    {
        public int QId { get; set; }
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        public string Comprehensive { get; set; }
    
        public virtual Questions Questions { get; set; }
        public virtual Questions Questions1 { get; set; }
        public virtual Questions Questions2 { get; set; }
    }
}
