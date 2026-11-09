namespace HappyPet.Application.Common
{
    /// <summary>
    /// Great-circle distance helpers used by the lost-pet search.
    /// </summary>
    public static class GeoDistance
    {
        private const double EarthRadiusKm = 6371.0088;
        private const double KmPerDegreeLatitude = 110.574;
        private const double KmPerDegreeLongitudeAtEquator = 111.320;

        /// <summary>
        /// Haversine distance between two coordinates in kilometres.
        /// </summary>
        public static double CalculateKm(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var lat1 = ToRadians(latitude1);
            var lat2 = ToRadians(latitude2);
            var deltaLat = ToRadians(latitude2 - latitude1);
            var deltaLon = ToRadians(longitude2 - longitude1);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                    + Math.Cos(lat1) * Math.Cos(lat2)
                    * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            return 2 * EarthRadiusKm * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        /// <summary>
        /// Latitude/longitude bounding box around a point and radius, used to prefilter rows in SQL.
        /// </summary>
        public static BoundingBox GetBoundingBox(double latitude, double longitude, double radiusKm)
        {
            var latitudeDelta = radiusKm / KmPerDegreeLatitude;
            var minLatitude = Math.Max(-90, latitude - latitudeDelta);
            var maxLatitude = Math.Min(90, latitude + latitudeDelta);

            var cosLatitude = Math.Cos(ToRadians(latitude));
            var longitudeDelta = Math.Abs(cosLatitude) < 1e-6
                ? 180
                : radiusKm / (KmPerDegreeLongitudeAtEquator * Math.Abs(cosLatitude));

            if (longitudeDelta >= 180)
            {
                return new BoundingBox(minLatitude, maxLatitude, -180, 180);
            }

            var minLongitude = longitude - longitudeDelta;
            var maxLongitude = longitude + longitudeDelta;

            if (minLongitude < -180)
            {
                minLongitude += 360;
            }

            if (maxLongitude > 180)
            {
                maxLongitude -= 360;
            }

            return new BoundingBox(minLatitude, maxLatitude, minLongitude, maxLongitude);
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        public readonly record struct BoundingBox(
            double MinLatitude,
            double MaxLatitude,
            double MinLongitude,
            double MaxLongitude)
        {
            /// <summary>True when the box crosses the antimeridian.</summary>
            public bool CrossesAntimeridian => MinLongitude > MaxLongitude;
        }
    }
}
