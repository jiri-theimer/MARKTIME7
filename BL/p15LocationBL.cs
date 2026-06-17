using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip15LocationBL
    {
        public BO.p15Location Load(int pid);
        public IEnumerable<BO.p15Location> GetList(BO.myQueryP15 mq);
        public int Save(BO.p15Location rec);

        public BO.Result AppendWeatherLog(string record_prefix, int record_pid, double lon = 0, double lat = 0);    //v pid se vrací hodnota j95id
        public BO.j95GeoWeatherLog LoadWeatherRec(int j95id);
        public BO.j95GeoWeatherLog LoadWeatherRec(string record_prefix, int record_pid);
        

    }
    class p15LocationBL : BaseBL, Ip15LocationBL
    {
        public p15LocationBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,own.j02LastName+' '+own.j02FirstName as Owner,");
            sb(_db.GetSQL1_Ocas("p15"));
            sb(" FROM p15Location a LEFT OUTER JOIN j02User own ON a.j02ID_Owner=own.j02ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p15Location Load(int pid)
        {
            return _db.Load<BO.p15Location>(GetSQL1(" WHERE a.p15ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p15Location> GetList(BO.myQueryP15 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p15Location>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p15Location rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
            p.AddInt("x04ID", rec.x04ID,true);
            p.AddString("p15Name", rec.p15Name);
            p.AddString("p15Notepad", rec.p15Notepad);
            p.AddString("p15Street", rec.p15Street);
            p.AddString("p15City", rec.p15City);
            p.AddString("p15PostCode", rec.p15PostCode);
            p.AddString("p15Country", rec.p15Country);
            p.AddDouble("p15Longitude", rec.p15Longitude);
            p.AddDouble("p15Latitude", rec.p15Latitude);

            return _db.SaveRecord("p15Location", p, rec);

        }
        private bool ValidateBeforeSave(BO.p15Location rec)
        {
            if (string.IsNullOrEmpty(rec.p15Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p15Longitude==0.00 || rec.p15Latitude == 0.00)
            {
                this.AddMessage("Chybí vyplnit souřadnice."); return false;
            }


            return true;
        }


        public BO.Result AppendWeatherLog(string record_prefix, int record_pid,double lon=0,double lat=0) 
        {
            if (lon > 0 && lat > 0)
            {
                var geo = new BL.Code.WeatherSupport().LoadWeatherPlace(lon, lat).Result;
                var ret = new BO.Result(false);
                ret.pid = SaveGeo2WeatherLog(geo,0);
                return ret;
            }
            if (lon==0 || lat == 0)
            {
                int intP15ID = 0;   //najít pojmenovanou lokalitu
                switch (record_prefix)
                {
                    case "o23":                        
                        var lisO19 = _mother.o23DocBL.GetList_o19(record_pid).Where(p => p.o19RecordPid > 0 && (p.o20Entity == "p15" || p.o20Entity == "p41" || p.o20Entity == "p56"));
                        if (lisO19.Any(p => p.o20Entity == "p15"))
                        {
                            intP15ID = _mother.p15LocationBL.Load(lisO19.First().o19RecordPid).pid;
                        }
                        if (intP15ID==0 && lisO19.Any(p => p.o20Entity == "p41"))
                        {
                            intP15ID = _mother.p41ProjectBL.Load(lisO19.First().o19RecordPid).p15ID;
                        }
                        if (intP15ID==0 && lisO19.Any(p => p.o20Entity == "p56"))
                        {
                            intP15ID = _mother.p56TaskBL.Load(lisO19.First().o19RecordPid).p15ID;

                        }
                        break;
                    case "p41":
                        var recP41 = _mother.p41ProjectBL.Load(record_pid);
                        intP15ID = recP41.p15ID;
                        break;
                    case "p56":
                        var recP56 = _mother.p56TaskBL.Load(record_pid);
                        intP15ID = recP56.p15ID;
                        if (intP15ID == 0 && recP56.p41ID > 0)
                        {
                            intP15ID = _mother.p41ProjectBL.Load(recP56.p41ID).p15ID;
                        }
                        break;
                    case "p15":
                        intP15ID = record_pid;
                        break;
                }
                if (intP15ID > 0)
                {
                    var recP15 = _mother.p15LocationBL.Load(intP15ID);
                    var geo = new BL.Code.WeatherSupport().LoadWeatherPlace(recP15.p15Longitude, recP15.p15Latitude).Result;

                    var ret = new BO.Result(false);
                    ret.pid = SaveGeo2WeatherLog(geo, intP15ID);
                    return ret;

                }
                
            }

            return new BO.Result(true, "Záznam nemá vazbu na pojmenovanou lokalitu nebo na vstupu chybí souřadnice.");


        }


        private int SaveGeo2WeatherLog(BO.Geo.WeatherPlace wp, int p15id)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("x01ID", _mother.CurrentUser.x01ID);
            p.AddInt("p15ID", p15id,true);
            p.AddInt("j02ID", _mother.CurrentUser.pid, true);
            p.AddDateTime("j95Date", DateTime.Now);
            
            p.AddDouble("j95Longitude", wp.LongitudeInput);
            p.AddDouble("j95Latitude", wp.LatitudeInput);
            p.AddDouble("j95Temp", wp.Temp);
            p.AddDouble("j95TempFeelsLike", wp.TempFeelsLike);
            p.AddDouble("j95TempMin", wp.TempMin);
            p.AddDouble("j95TempMax", wp.TempMax);
            p.AddInt("j95Pressure", wp.Pressure);
            p.AddInt("j95Humidity", wp.Humidity);
            p.AddInt("j95Visibility", wp.Visibility);
            p.AddDouble("j95WindSpeed", wp.WindSpeed);
            p.AddInt("j95WindDeg", wp.WindDeg);
            p.AddDouble("j95WindGust", wp.WindGust);
            p.AddString("j95Country", wp.Country);
            p.AddString("j95Location", wp.Location);
            p.AddString("j95Icon", wp.Icon);
            p.AddString("j95Description", wp.Description);

            p.AddString("j95ErrorMessage", wp.ErrorMessage);

            string s = "INSERT INTO j95GeoWeatherLog(x01ID,p15ID,j02ID,j95Date,j95Longitude,j95Latitude,j95Temp,j95TempFeelsLike,j95TempMin,j95TempMax,j95Pressure,j95Humidity,j95Visibility,j95WindSpeed,j95WindDeg,j95WindGust,j95Country,j95Location,j95Icon,j95Description,j95ErrorMessage)";
            s += " VALUES(@x01ID,@p15ID,@j02ID,@j95Date,@j95Longitude,@j95Latitude,@j95Temp,@j95TempFeelsLike,@j95TempMin,@j95TempMax,@j95Pressure,@j95Humidity,@j95Visibility,@j95WindSpeed,@j95WindDeg,@j95WindGust,@j95Country,@j95Location,@j95Icon,@j95Description,@j95ErrorMessage)";
            s += ";SELECT CAST(SCOPE_IDENTITY() as int) as Value";

            return _db.Load<BO.GetInteger>(s, p.getDynamicDapperPars()).Value;
            //return _db.RunSql(s, p.getDynamicDapperPars());

        }

        public BO.j95GeoWeatherLog LoadWeatherRec(int j95id)
        {
            return _db.Load<BO.j95GeoWeatherLog>("SELECT a.* FROM j95GeoWeatherLog a WHERE a.j95ID=@pid", new { pid = j95id });

        }
        public BO.j95GeoWeatherLog LoadWeatherRec(string record_prefix, int record_pid)
        {
            string s = "SELECT TOP 1 a.* FROM j95GeoWeatherLog a";
            switch (record_prefix)
            {
                case "j95":
                    return LoadWeatherRec(record_pid);                  
                case "p15":
                    s += $" WHERE a.p15ID={record_pid}";
                    break;
                case "p41":
                    s += $" WHERE a.p15ID IN (SELECT p15ID FROM p41Project WHERE p41ID={record_pid})";
                    break;
                case "p56":
                    s += $" WHERE a.p15ID IN (SELECT p15ID FROM p56Task WHERE p56ID={record_pid})";
                    break;
                case "o23":
                    s += $" WHERE a.j95ID IN (SELECT j95ID FROM o23Doc WHERE o23ID={record_pid})";
                    break;
            }
            
            s += " ORDER BY a.j95ID DESC";
            return _db.Load<BO.j95GeoWeatherLog>(s, new { prefix = record_prefix, pid = record_pid });

        }

    }
}
