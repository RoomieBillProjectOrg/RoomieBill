using System;
using System.Text.Json.Serialization;

namespace Roomiebill.Server.Models;

public class BillData {
    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }
}
