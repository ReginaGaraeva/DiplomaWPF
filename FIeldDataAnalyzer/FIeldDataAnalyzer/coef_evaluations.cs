//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FIeldDataAnalyzer
{
    using System;
    using System.Collections.Generic;
    
    public partial class coef_evaluations
    {
        public coef_evaluations()
        {
            this.final_gather_point_measurements = new HashSet<final_gather_point_measurements>();
        }
    
        public int coef_evaluations_id { get; set; }
        public System.DateTime date_from { get; set; }
        public System.DateTime date_to { get; set; }
        public float Kt { get; set; }
        public float Kp { get; set; }
    
        public virtual ICollection<final_gather_point_measurements> final_gather_point_measurements { get; set; }
    }
}