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
    
    public partial class StudentTestLog
    {
        public int LogId { get; set; }
        public System.Guid TestId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public int PaperId { get; set; }
        public int Level { get; set; }
        public int QId { get; set; }
        public Nullable<int> SelectedOption { get; set; }
        public string MultiLineAnswer { get; set; }
        public bool isCorrected { get; set; }
    }
}
