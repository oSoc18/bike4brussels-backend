using rideaway_backend.Exceptions;
using Itinero.LocalGeo;
using System;
using GeoJSON.Net.Geometry;

namespace rideaway_backend.Util {
    public class Utility {
        /// <summary>
        /// Turns a string consisting of a latitude and longitude value seperated 
        /// by a comma into a Coordinate.
        /// </summary>
        /// <param name="coord">String to convert.</param>
        /// <returns>A coordinate object.</returns>
        /// <exception name="CoordinateLengthException">
        /// Thrown when the string is null or doesn't consist of two parts.
        /// </exception>
        /// <exception name="CoordinateParseException">
        /// Thrown when one part of the string could not be parsed to a float.
        /// </exception>
        public static Coordinate ParseCoordinate (string coord) {
            float lat;
            float lon;
            if (coord == null) {
                throw new CoordinateLengthException ("Location is not specified");
            }
            string[] parts = coord.Split (',');
            if (parts.Length != 2) {
                throw new CoordinateLengthException ("Location <" + coord + "> does not consist of two parts");
            }
            if (float.TryParse (parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                float.TryParse (parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon)) {
                return new Coordinate (lat, lon);
            } else {
                throw new CoordinateParseException ("Location <" + coord + "> could not be parsed");
            }
        }

        /// <summary>
        /// Convert a Coordinate from Lambert72 to WGS84 coordinate system.
        /// </summary>
        /// <param name="x">Longitude of Lambert72 coordinate pair</param>
        /// <param name="y">Latitude of Lambert72 coordinate pair</param>
        /// <returns>Position in WSG84 coordinate system</returns>
        public static Position Lambert72toWGS84 (double x, double y) {
            double newLongitude;
            double newLatitude;

            double n = 0.77164219;
            double F = 1.81329763;
            double thetaFudge = 0.00014204;
            double e = 0.08199189;
            double a = 6378388;
            double xDiff = 149910;
            double yDiff = 5400150;

            double theta0 = 0.07604294;

            double xReal = xDiff - x;
            double yReal = yDiff - y;

            double rho = Math.Sqrt (xReal * xReal + yReal * yReal);
            double theta = Math.Atan (xReal / -yReal);

            newLongitude = (theta0 + (theta + thetaFudge) / n) * 180 / Math.PI;
            newLatitude = 0;

            for (int i = 0; i < 5; ++i) {
                newLatitude = (2 * Math.Atan (Math.Pow (F * a / rho, 1 / n) * Math.Pow ((1 + e * Math.Sin (newLatitude)) / (1 - e * Math.Sin (newLatitude)), e / 2))) - Math.PI / 2;
            }
            newLatitude *= 180 / Math.PI;
            return new Position (newLatitude, newLongitude);
        }

        /// <summary>
        /// Convert a Coordinate from WSG84 to Lambert72 coordinate system.
        /// </summary>
        /// <param name="Lat">Latitude of WSG84 coordinate pair</param>
        /// <param name="Lng">Longitude of WSG84 coordinate pair</param>
        /// <returns>Position in the Lamber72 coordinate system</returns>
        public static Position WSG84toLambert72 (double Lat, double Lng) {
            // Algorithm as found on http://zoologie.umons.ac.be/tc/algorithms.aspx
            // Venture no further as this quest will take to much of your stamina

            //
            //Input parameters : Lat, Lng : latitude / longitude in decimal degrees and in WGS84 datum
            //Output parameters : LatBel, LngBel : latitude / longitude in decimal degrees and in Belgian datum
            //

            int Haut = 0; //Altitude
            double LatBel = 0;
            double LngBel = 0;
            double DLat = 0;
            double DLng = 0;
            double Dh = 0;
            double dy = 0;
            double dx = 0;
            double dz = 0;
            double da = 0;
            double df = 0;
            double LWa = 0;
            double Rm = 0;
            double Rn = 0;
            double LWb = 0;
            double LWf = 0;
            double LWe2 = 0;
            double SinLat = 0;
            double SinLng = 0;
            double CoSinLat = 0;
            double CoSinLng = 0;

            double Adb = 0;

            //conversion to radians
            Lat = (Math.PI / 180) * Lat;
            Lng = (Math.PI / 180) * Lng;

            SinLat = Math.Sin (Lat);
            SinLng = Math.Sin (Lng);
            CoSinLat = Math.Cos (Lat);
            CoSinLng = Math.Cos (Lng);

            dx = 125.8;
            dy = -79.9;
            dz = 100.5;
            da = 251.0;
            df = 1.4192702E-05;

            LWf = 1 / 297;
            LWa = 6378388;
            LWb = (1 - LWf) * LWa;
            LWe2 = (2 * LWf) - (LWf * LWf);
            Adb = 1 / (1 - LWf);

            Rn = LWa / System.Math.Sqrt (1 - LWe2 * SinLat * SinLat);
            Rm = LWa * (1 - LWe2) / Math.Pow ((1 - LWe2 * Lat * Lat), 1.5);

            DLat = -dx * SinLat * CoSinLng - dy * SinLat * SinLng + dz * CoSinLat;
            DLat = DLat + da * (Rn * LWe2 * SinLat * CoSinLat) / LWa;
            DLat = DLat + df * (Rm * Adb + Rn / Adb) * SinLat * CoSinLat;
            DLat = DLat / (Rm + Haut);

            DLng = (-dx * SinLng + dy * CoSinLng) / ((Rn + Haut) * CoSinLat);
            Dh = dx * CoSinLat * CoSinLng + dy * CoSinLat * SinLng + dz * SinLat;
            Dh = Dh - da * LWa / Rn + df * Rn * Lat * Lat / Adb;

            LatBel = ((Lat + DLat) * 180) / Math.PI;
            LngBel = ((Lng + DLng) * 180) / Math.PI;

            //
            //       Conversion from spherical coordinates to Lambert 72
            //       Input parameters : lat, lng (spherical coordinates)
            //       Spherical coordinates are in decimal degrees converted to Belgium datum!
            // 
            double lat = LatBel;
            double lng = LngBel;

            const double LongRef = 0.076042943;
            //=4°21'24"983
            const double bLamb = 6378388 * (1 - (1 / 297));
            double aCarre = Math.Pow (6378388, 2);
            double eCarre = (aCarre - Math.Pow (bLamb, 2)) / aCarre;
            const double KLamb = 11565915.812935;
            const double nLamb = 0.7716421928;

            double eLamb = Math.Sqrt (eCarre);
            double eSur2 = eLamb / 2;

            //conversion to radians
            lat = (Math.PI / 180) * lat;
            lng = (Math.PI / 180) * lng;

            double eSinLatitude = eLamb * Math.Sin (lat);
            double TanZDemi = (Math.Tan ((Math.PI / 4) - (lat / 2))) * (Math.Pow (((1 + (eSinLatitude)) / (1 - (eSinLatitude))), (eSur2)));

            double RLamb = KLamb * (Math.Pow ((TanZDemi), nLamb));

            double Teta = nLamb * (lng - LongRef);

            double x = 0;
            double y = 0;

            x = 150000 + 0.01256 + RLamb * Math.Sin (Teta - 0.000142043);
            //for some reason there is an inaccuracy of 21020 m in the y direction
            y = 5400000 - 21020 + 88.4378 - RLamb * Math.Cos (Teta - 0.000142043);

            return new Position (y, x);
        }
    }
}