

namespace BL.Singleton.ColumnsProvider
{
    public class fpColumnsProvider: ColumnsProviderBase
    {
        public fpColumnsProvider()
        {
            //this.EntityName = "fp1";
            //this.CurrentFieldGroup = null;
            //oc = AF("PlanVydaje", "Plán výdajů", "a.PlanVydaje", "num", true); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true;
            //oc = AF("PlanOdmeny", "Plán odměn", "a.PlanOdmeny", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("PlanZisk", "Plán zisk", "a.PlanZisk", "num", true); oc.NotShowRelInHeader = true;
            //this.EntityName = "fp1_p31";
            //oc = AF("Odmeny", "Odměny (RO)", "a.Odmeny_Vyka", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("Vydaje", "Výdaje (RV)", "a.Vydaje_Vyka", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("HonorarInterni", "Na honorář (NH)", "a.Honorar_Interni", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("HonorarFakturacni", "Fa honorář", "a.Honorar_Fakturacni", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("Hodiny", "Hodiny", "a.Hodiny_Vyka", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("HodinyFa", "Hodiny Fa", "a.Hodiny_VykaFa", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("HodinyNefa", "Hodiny Nefa", "a.Hodiny_VykaNefa", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("VydajePlusHodiny", "RV+NH", "isnull(a.Vydaje_Vyka,0)+isnull(a.Honorar_Interni,0)", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("RozdilOdmeny", "RO-PO", "isnull(a.Odmeny_Vyka,0)-PlanOdmeny", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("RozdilVydaje1", "RV-PV", "isnull(a.Vydaje_Vyka,0)-PlanVydaje", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("RozdilVydaje2", "RV+NH-PV", "isnull(a.Vydaje_Vyka,0)+isnull(a.Honorar_Interni,0)-isnull(PlanVydaje,0)", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("Zisk1", "Zisk: RO-RV-NH", "isnull(a.Odmeny_Vyka,0)-isnull(a.Vydaje_Vyka,0)-isnull(a.Honorar_Interni,0)", "num", true); oc.NotShowRelInHeader = true;

            
            for(int i = 1; i <= 3; i++)
            {
                this.EntityName = $"fp{i}";
                this.CurrentFieldGroup = null;
                oc = AF("PlanVydaje", "Plán výdajů", "a.PlanVydaje", "num", true); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true;
                oc = AF("PlanOdmeny", "Plán odměn", "a.PlanOdmeny", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("PlanZisk", "Plán zisk", "a.PlanZisk", "num", true); oc.NotShowRelInHeader = true;
                this.EntityName = $"fp{i}_p31";
                oc = AF("Odmeny", "Odměny (RO)", "a.Odmeny_Vyka", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("Vydaje", "Výdaje (RV)", "a.Vydaje_Vyka", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("HonorarInterni", "Na honorář (NH)", "a.Honorar_Interni", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("HonorarFakturacni", "Fa honorář", "a.Honorar_Fakturacni", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("Hodiny", "Hodiny", "a.Hodiny_Vyka", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("HodinyFa", "Hodiny Fa", "a.Hodiny_VykaFa", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("HodinyNefa", "Hodiny Nefa", "a.Hodiny_VykaNefa", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("VydajePlusHodiny", "RV+NH", "isnull(a.Vydaje_Vyka,0)+isnull(a.Honorar_Interni,0)", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("RozdilOdmeny", "RO-PO", "isnull(a.Odmeny_Vyka,0)-PlanOdmeny", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("RozdilVydaje1", "RV-PV", "isnull(a.Vydaje_Vyka,0)-PlanVydaje", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("RozdilVydaje2", "RV+NH-PV", "isnull(a.Vydaje_Vyka,0)+isnull(a.Honorar_Interni,0)-isnull(PlanVydaje,0)", "num", true); oc.NotShowRelInHeader = true;
                oc = AF("Zisk1", "Zisk: RO-RV-NH", "isnull(a.Odmeny_Vyka,0)-isnull(a.Vydaje_Vyka,0)-isnull(a.Honorar_Interni,0)", "num", true); oc.NotShowRelInHeader = true;

            }

            this.EntityName = "fp2_r01";
            oc = AF("PlanHodiny", "Plán hodin", "a.PlanHodiny", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("PlanHodinyFa", "Plán Fa hodin", "a.PlanHodinyFa", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("PlanHodinyNefa", "Plán Nefa hodin", "a.PlanHodinyNefa", "num", true); oc.NotShowRelInHeader = true;

        }
    }
}
