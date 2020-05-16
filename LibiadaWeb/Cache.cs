using LibiadaWeb;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

/// <summary>
/// Custom cache for storing matter table data.
/// </summary>
public class Cache
{
    /// <summary>
    /// The list of matters.
    /// </summary>
    public List<Matter> Matters { get; set; }

    /// <summary>
    /// The instance for singleton.
    /// </summary>
    private static Cache instance = null;

    /// <summary>
    /// Initializes list of matters.
    /// </summary>
    private Cache()
	{
        using (var db = new LibiadaWebEntities())
        {
            Matters = db.Matter.ToList();
        }
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    public static Cache GetInstance()
    {
        if (instance == null)
        {
            instance = new Cache();
        }
        return instance;
    }

    /// <summary>
    /// Clears the instance.
    /// </summary>
    public static void Clear()
    {
        instance = null;
    }
}
