

namespace BO
{
    public class InitMyQuery
    {
        private List<string> _mqi { get; set; }
        private string _prefix { get; set; }
        private string _master_prefix { get; set; }
        private int _master_pid { get; set; }
        private string _rez { get; set; }
        private BO.RunningUser _CurrentUser;
        public InitMyQuery(BO.RunningUser ru)
        {
            _CurrentUser = ru;
        }

        private void handle_myqueryinline_input(string myqueryinline)
        {
            if (string.IsNullOrEmpty(myqueryinline))
            {
                return;
            }
            _mqi = Code.Bas.ConvertString2List(myqueryinline, "|");

        }
        public BO.baseQuery Load(string prefix,string master_prefix = null, int master_pid = 0, string myqueryinline = null,string rez=null)
        {
            handle_myqueryinline_input(myqueryinline);
            _master_prefix = validate_prefix(master_prefix);
            _master_pid = master_pid;
            _rez = rez;
            _prefix = prefix.Substring(0, 3);
            
           
            switch (_prefix)
            {
                case "c26":
                    return handle_myquery_reflexe(new BO.myQueryC26() { CurrentUser= _CurrentUser });
                case "p31":
                    return handle_myquery_reflexe(new BO.myQueryP31() { CurrentUser = _CurrentUser });
                case "p32":
                    return handle_myquery_reflexe(new BO.myQueryP32() { CurrentUser = _CurrentUser });
                case "p34":
                    return handle_myquery_reflexe(new BO.myQueryP34() { CurrentUser = _CurrentUser });
                case "p42":
                    return handle_myquery_reflexe(new BO.myQueryP42() { CurrentUser = _CurrentUser });
                case "p28":
                    return handle_myquery_reflexe(new BO.myQueryP28() { CurrentUser = _CurrentUser });
                case "j02":
                    return handle_myquery_reflexe(new BO.myQueryJ02() { CurrentUser = _CurrentUser });
                case "j04":
                    return handle_myquery_reflexe(new BO.myQueryJ04() { CurrentUser = _CurrentUser });
                case "j11":
                    return handle_myquery_reflexe(new BO.myQueryJ11() { CurrentUser = _CurrentUser });
                case "j61":
                    return handle_myquery_reflexe(new BO.myQueryJ61() { CurrentUser = _CurrentUser });
                case "o22":
                    return handle_myquery_reflexe(new BO.myQueryO22() { CurrentUser = _CurrentUser });
                case "o23":
                    return handle_myquery_reflexe(new BO.myQueryO23() { CurrentUser = _CurrentUser });
                case "o27":
                    return handle_myquery_reflexe(new BO.myQueryO27() { CurrentUser = _CurrentUser });
                case "o43":
                    return handle_myquery_reflexe(new BO.myQueryO43() { CurrentUser = _CurrentUser });
                case "r01":
                    return handle_myquery_reflexe(new BO.myQueryR01() { CurrentUser = _CurrentUser });
                case "j40":
                    return handle_myquery_reflexe(new BO.myQueryJ40() { CurrentUser = _CurrentUser });
                case "p15":
                    return handle_myquery_reflexe(new BO.myQueryP15() { CurrentUser = _CurrentUser });
                case "p12":
                    return handle_myquery_reflexe(new BO.myQueryP12() { CurrentUser = _CurrentUser });
                case "p51":
                    return handle_myquery_reflexe(new BO.myQueryP51() { CurrentUser = _CurrentUser });
                case "p56":
                    return handle_myquery_reflexe(new BO.myQueryP56() { CurrentUser = _CurrentUser });
                case "p58":
                    return handle_myquery_reflexe(new BO.myQueryP58() { CurrentUser = _CurrentUser });
                case "p75":
                    return handle_myquery_reflexe(new BO.myQueryP75() { CurrentUser = _CurrentUser });
                case "p90":
                    return handle_myquery_reflexe(new BO.myQueryP90() { CurrentUser = _CurrentUser });
                case "p91":
                    return handle_myquery_reflexe(new BO.myQueryP91() { CurrentUser = _CurrentUser });
                case "p92":
                    return handle_myquery_reflexe(new BO.myQueryP92() { CurrentUser = _CurrentUser });
                case "p39":
                    return handle_myquery_reflexe(new BO.myQueryP39() { CurrentUser = _CurrentUser });
                case "p40":
                    return handle_myquery_reflexe(new BO.myQueryP40() { CurrentUser = _CurrentUser });
                case "p41":
                    return handle_myquery_reflexe(new BO.myQueryP41("p41") { CurrentUser = _CurrentUser });
                case "le1":
                    return handle_myquery_reflexe(new BO.myQueryP41("le1") { p07level = 1, CurrentUser = _CurrentUser });
                case "le2":
                    return handle_myquery_reflexe(new BO.myQueryP41("le2") { p07level = 2, CurrentUser = _CurrentUser });
                case "le3":
                    return handle_myquery_reflexe(new BO.myQueryP41("le3") { p07level = 3, CurrentUser = _CurrentUser });
                case "le4":
                    return handle_myquery_reflexe(new BO.myQueryP41("le4") { p07level = 4, CurrentUser = _CurrentUser });
                case "le5":
                    return handle_myquery_reflexe(new BO.myQueryP41("le5") { p07level = 5, CurrentUser = _CurrentUser });
                case "p84":
                    return handle_myquery_reflexe(new BO.myQueryP84() { CurrentUser = _CurrentUser });
                case "p11":
                    return handle_myquery_reflexe(new BO.myQueryP11() { CurrentUser = _CurrentUser });
                case "b05":
                    return handle_myquery_reflexe(new BO.myQueryB05() { CurrentUser = _CurrentUser });
                case "o18":
                    return handle_myquery_reflexe(new BO.myQueryO18() { CurrentUser = _CurrentUser });
                case "x31":
                    return handle_myquery_reflexe(new BO.myQueryX31() { CurrentUser = _CurrentUser });
                case "p24":
                    return handle_myquery_reflexe(new BO.myQueryP24() { CurrentUser = _CurrentUser });
                case "x40":
                    return handle_myquery_reflexe(new BO.myQueryX40() { CurrentUser = _CurrentUser });
                case "j90":
                    return handle_myquery_reflexe(new BO.myQueryJ90() { CurrentUser = _CurrentUser });
                case "j92":
                    return handle_myquery_reflexe(new BO.myQueryJ92() { CurrentUser = _CurrentUser });
                case "j95":
                    return handle_myquery_reflexe(new BO.myQueryJ95() { CurrentUser = _CurrentUser });
                case "p49":
                    return handle_myquery_reflexe(new BO.myQueryP49() { CurrentUser = _CurrentUser });
                default:
                    return handle_myquery_reflexe(new BO.myQuery(prefix.Substring(0, 3)) { CurrentUser = _CurrentUser });
            }
        }

        private T handle_myquery_reflexe<T>(T mq)
        {
            if (_mqi != null)
            {   //na vstupu je explicitní myquery ve tvaru název|typ|hodnota
                for (int i = 0; i < _mqi.Count; i += 3)
                {

                    switch (_mqi[i + 1])
                    {
                        case "int":
                            Code.Reflexe.SetPropertyValue(mq, _mqi[i], Convert.ToInt32(_mqi[i + 2]));
                            break;
                        case "date":
                            Code.Reflexe.SetPropertyValue(mq, _mqi[i], Code.Bas.String2Date(_mqi[i + 2]));
                            break;
                        case "bool":
                            Code.Reflexe.SetPropertyValue(mq, _mqi[i], Code.Bas.BG(_mqi[i + 2]));

                            break;
                        case "list_int":
                            Code.Reflexe.SetPropertyValue(mq, _mqi[i], Code.Bas.ConvertString2ListInt(_mqi[i + 2]));
                            break;
                        default:
                            Code.Reflexe.SetPropertyValue(mq, _mqi[i], _mqi[i + 2]);
                            break;
                    }
                }
            }

            //filtr podle master_prefix+master_pid
            if (_master_pid > 0 && _master_prefix != null)
            {
                switch (_master_prefix)
                {
                    case "le5":
                        Code.Reflexe.SetPropertyValue(mq, "p41id", _master_pid);
                        break;
                    case "le4":
                        Code.Reflexe.SetPropertyValue(mq, "leindex", 4);
                        Code.Reflexe.SetPropertyValue(mq, "lepid", _master_pid);
                        break;
                    case "le3":
                        Code.Reflexe.SetPropertyValue(mq, "leindex", 3);
                        Code.Reflexe.SetPropertyValue(mq, "lepid", _master_pid);
                        break;
                    case "le2":
                        Code.Reflexe.SetPropertyValue(mq, "leindex", 2);
                        Code.Reflexe.SetPropertyValue(mq, "lepid", _master_pid);
                        break;
                    case "le1":
                        Code.Reflexe.SetPropertyValue(mq, "leindex", 1);
                        Code.Reflexe.SetPropertyValue(mq, "lepid", _master_pid);
                        break;
                    default:
                        Code.Reflexe.SetPropertyValue(mq, $"{_master_prefix}id", _master_pid);
                        break;
                }                

                Code.Reflexe.SetPropertyValue(mq, "master_prefix", _master_prefix);
            }

            if (!string.IsNullOrEmpty(_rez))
            {
                switch (_prefix)
                {
                    case "o23":
                        Code.Reflexe.SetPropertyValue(mq, "o17id", Convert.ToInt32(_rez));  //filtrování podle agenda-menu o17ID
                        break;
                }
            }



            return mq;
        }








        private string validate_prefix(string s = null)
        {
            if (s != null)
            {
                s = s.Substring(0, 3);
            }

            return s;
        }



        public void Handle_GroupBy_ExplicitSqlWhere(BO.TheGridState gridstate, BO.TheGridColumn TheGridGroupByCol, BO.baseQuery mq)    //sql where klauzule pro groupby vybraný souhrn
        {
            string strSqlWhere = null;
            if (gridstate.j75GroupLastValue == "null")
            {
                strSqlWhere = $"{TheGridGroupByCol.getFinalSqlSyntax_GROUPBY()} IS NULL";
            }
            else
            {
                string strSqlValue = gridstate.j75GroupLastValue;
                switch (TheGridGroupByCol.FieldType)
                {
                    case "num":
                        strSqlValue = strSqlValue.Replace(",", ".");
                        break;
                    case "datetime":
                    case "date":
                        strSqlValue = $"convert(datetime,'{strSqlValue}',104)";
                        break;
                    case "bool":
                        if (strSqlValue == "true" || strSqlValue == "True")
                        {
                            strSqlValue = "1";
                        }
                        else
                        {
                            strSqlValue = "0";
                        }
                        break;
                    case "string":
                        strSqlValue = BO.Code.Bas.GS(strSqlValue);
                        break;
                }
                strSqlWhere = $"{TheGridGroupByCol.getFinalSqlSyntax_GROUPBY()}={strSqlValue}";
            }

            if (mq.explicit_sqlwhere == null)
            {
                mq.explicit_sqlwhere = strSqlWhere;
            }
            else
            {
                mq.explicit_sqlwhere = $"({mq.explicit_sqlwhere}) AND {strSqlWhere}";
            }
        }

    }
}
