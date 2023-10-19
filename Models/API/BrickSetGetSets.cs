using System;
using System.Collections.Generic;

namespace BrikBotCore.Models.API
{
	public class AgeRange
    {
	    int min { get; set; }
	    int max { get; set; }
    }

	public class Barcode
	{
		public string EAN { get; set; }
	}

    public class Collection
    {
	    string minifigNumber { get; set; }
	    string name { get; set; }
	    string category { get; set; }
	    int ownedInSets { get; set; }
	    int ownedLoose { get; set; }
	    int ownedTotal { get; set; }
	    bool wanted { get; set; }
    }

    public class Collections
    {
        public int ownedBy { get; set; }
        public int wantedBy { get; set; }
    }

    public class Dimensions
    {
	    public double height { get; set; }
	    public double width { get; set; }
	    public double depth { get; set; }
	    public double weight { get; set; }
    }

    public class ExtendedData
    {
        public string notes { get; set; }
        public List<string> tags { get; set; }
        public string description { get; set; }
    }

    public class Image
    {
        public string thumbnailURL { get; set; }
        public string imageURL { get; set; }
    }

    public class LEGOCom
    {
        public US US { get; set; }
        public UK UK { get; set; }
        public CA CA { get; set; }
        public DE DE { get; set; }
    }

    public class BrickSetGetSets
    {
        public string status { get; set; }
        public int matches { get; set; }
        public List<Set> sets { get; set; }
    }

    public class Set
    {
        public int setID { get; set; }
        public string number { get; set; }
        public int numberVariant { get; set; }
        public string name { get; set; }
        public int year { get; set; }
        public string theme { get; set; }
        public string themeGroup { get; set; }
        public string category { get; set; }
        public bool released { get; set; }
        public int pieces { get; set; }
        public int minifigs { get; set; }
        public Image image { get; set; }
        public string bricksetURL { get; set; }
        public Collection collection { get; set; }
        public Collections collections { get; set; }
        public LEGOCom LEGOCom { get; set; }
        public double rating { get; set; }
        public int reviewCount { get; set; }
        public string packagingType { get; set; }
        public string availability { get; set; }
        public int instructionsCount { get; set; }
        public int additionalImageCount { get; set; }
        public AgeRange ageRange { get; set; }
        public Dimensions dimensions { get; set; }
        public Barcode barcode { get; set; }
        public ExtendedData extendedData { get; set; }
        public DateTime lastUpdated { get; set; }
    }

    public class UK
    {
	    public double retailPrice { get; set; }
	    public DateTime dateFirstAvailable { get; set; }
    }

    public class US
    {
	    public double retailPrice { get; set; }
	    public DateTime dateFirstAvailable { get; set; }
    }
    
    public class CA
    {
	    public double retailPrice { get; set; }
	    public DateTime dateFirstAvailable { get; set; }
    }
    
    public class DE
    {
	    public double retailPrice { get; set; }
	    public DateTime dateFirstAvailable { get; set; }
    }
}