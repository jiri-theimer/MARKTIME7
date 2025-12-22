

namespace BO.Code
{
    public static class Reflexe
    {
        private static string ErrorMessage;


        public static object GetPropertyValue(object obj, string PropName)
        {
            Type objType = obj.GetType();
            object PropValue = null;
            try
            {
                System.Reflection.PropertyInfo pInfo = objType.GetProperty(PropName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

                PropValue = pInfo.GetValue(obj, System.Reflection.BindingFlags.GetProperty, null, null, null);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return PropValue;
        }



        public static void SetPropertyValue(object obj, string PropName, object objNewValue)
        {
            Type objType = obj.GetType();
            System.Reflection.PropertyInfo pInfo = objType.GetProperty(PropName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);


            if (pInfo == null)
            {
                if (PropName == "global_d1")    //zděděný atribut z abstract třídy
                {
                    ((BO.baseQuery)obj).global_d1 = Convert.ToDateTime(objNewValue);
                    return;
                }
                if (PropName == "global_d2")    // zděděný atribut z abstract třídy
                {
                    ((BO.baseQuery)obj).global_d2 = Convert.ToDateTime(objNewValue);
                    return;
                }
                if (PropName == "period_field") // zděděný atribut z abstract třídy
                {
                    if (objNewValue == null) return;
                    ((BO.baseQuery)obj).period_field = Convert.ToString(objNewValue);
                    return;
                }
            }

            if (pInfo == null)
            {
                BO.Code.File.LogError("PropName: " + PropName+ ", objNewValue: "+ objNewValue, null, "Reflexe/SetPropertyValue");
            }
            if (pInfo.PropertyType.Name == "Int32")
            {
                if (objNewValue == null)
                {
                    objNewValue = 0;
                }
                else
                {
                    objNewValue = Convert.ToInt32(objNewValue);
                }

            }
            if (pInfo.PropertyType.Name == "Double")
            {
                if (objNewValue == null) { objNewValue = 0; } else { objNewValue = Convert.ToDouble(objNewValue); }
            };
            if (pInfo.PropertyType.Name == "Decimal")
            {
                if (objNewValue == null) { objNewValue = 0; } else { objNewValue = Convert.ToDecimal(objNewValue); }
            };
           
            
            pInfo.SetValue(obj, objNewValue, null);

        }

       

        
    }


}
