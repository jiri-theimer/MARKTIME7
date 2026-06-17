



function _rejstrik_justice(elementid)
{
    var ico = get_element_value(elementid);
    
    otevri_rejstrik(ico, "IČO", "https://or.justice.cz/ias/ui/rejstrik-$firma?ico=" + ico);
    
}
function _rejstrik_obchodny_register(elementid)
{
    var ico = get_element_value(elementid);
    otevri_rejstrik(ico, "IČO", "https://www.orsr.sk/hladaj_ico.asp?ICO=" + ico);
    
}
function _rejstrik_adis(elementid) {    
    var dic = get_element_value(elementid);
    otevri_rejstrik(dic, "DIČ", "https://plus.marktime.net/Adis?dic=" + dic);
    
}
function _rejstrik_aresinfo(elementid)
{
    var ico = get_element_value(elementid);
    
    otevri_rejstrik(ico, "IČO", "https://ares.gov.cz/ekonomicke-subjekty?ico=" + ico);
    
}
function _rejstrik_insolvence(elementid)
{
    var ico = get_element_value(elementid);
    
    var s = "https://isir.justice.cz/isir/ueu/vysledek_lustrace.do?ceuprob=x&mesto=&cislo_senatu=&bc_vec=&rocnik=&id_osoby_puvodce=&druh_stav_konkursu=&datum_stav_od=&datum_stav_do=&aktualnost=AKTUALNI_I_UKONCENA&druh_kod_udalost=&datum_akce_od=&datum_akce_do=&nazev_osoby_f=&nazev_osoby_spravce=&rowsAtOnce=50&spis_znacky_datum=&spis_znacky_obdobi=14DNI";
    s += "&ic=" + odkurovat(ico);
    otevri_rejstrik(ico, "IČO", s);
}

function _rejstrik_sverenecky_fond(elementid) {
    var ico = get_element_value(elementid);
    otevri_rejstrik(ico, "IČO", "https://esf.justice.cz/ias/isesf/rejstrik-$fond?ico=" + ico);

}

function _rejstrik_kurzycz(elementid) {
    var ico = get_element_value(elementid);

    otevri_rejstrik(ico,"IČO","https://rejstrik-firem.kurzy.cz/"+ico);
}


function get_element_value(elementid) {
    var c = $("#" + elementid);
    return odkurovat($(c).val());
}

function otevri_rejstrik(icodic_value,icodic_name,url)
{
    icodic_value = odkurovat(icodic_value);
    if (icodic_value == "") {
        alert("Na vstupu chybí ID subjektu (" + icodic_name + ")");
        return;
    }

    window.open(url, "_blank");
}

function odkurovat(s) {
    if (s == null || s == "") {
        return "";
    }
    s = s.replace(" ", "");

    return s;
}