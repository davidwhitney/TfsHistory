using System;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsHistory
{
    public static class Extensions
    {
        public static int? FirstWorkItem(this Changeset extended)
        {
            if(extended.WorkItems.Length > 0)
            {
                return extended.WorkItems[0].Id;
            }
            
            return Int32.MinValue;
        }
    }
}