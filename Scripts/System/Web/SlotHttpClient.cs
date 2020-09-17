using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

/// <summary>
/// Shark用HttpClient
/// </summary>
public static class SlotHttpClient
{
    /// <summary>
    /// instance
    /// </summary>
    public static readonly HttpClient instance = new HttpClient();
}