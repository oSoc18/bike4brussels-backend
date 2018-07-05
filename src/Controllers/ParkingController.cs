using System;
using Itinero.LocalGeo;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using rideaway_backend.Exceptions;
using rideaway_backend.Instance;
using rideaway_backend.Util;

namespace rideaway_backend.Controllers {
    /// <summary>
    /// Controller for the parking endpoint
    /// </summary>
    [Route ("[controller]")]
    public class ParkingController : Controller {
        /// <summary>
        /// Called on GET request to /parkings. Returns response containing bicycle parkings in a
        /// certain radius around the given location.
        /// </summary>
        /// <param name="loc">location</param>
        /// <param name="radius">radius to search parkings in meters</param>
        /// <returns>Response with parkings</returns>
        [HttpGet]
        [EnableCors ("AllowAnyOrigin")]
        public ActionResult Get (string loc, int radius = 500) {
            try {
                Coordinate location = Utility.ParseCoordinate (loc);

                return Json (ParkingInstance.getParkings (location, radius));

            } catch (ResolveException re) {
                return NotFound (re.Message);
            } catch (Exception e) {
                return BadRequest (e.Message);
            }
        }
    }
}