
public class OpenStreetMap
{
    public List<OSM_COOR> coors { get; set; }
}

public class OSM_COOR
{
    public int place_id { get; set; }
    public string licence { get; set; }
    public string osm_type { get; set; }
    public long osm_id { get; set; }
    public string[] boundingbox { get; set; }
    public string lat { get; set; }
    public string lon { get; set; }
    public string display_name { get; set; }
    public int place_rank { get; set; }
    public string category { get; set; }
    public string type { get; set; }
    public float importance { get; set; }
}
