﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yieldtome.Interfaces;
using yieldtome.API.Data.Objects;

namespace yieldtome.API.Data.Services
{
    [Export(typeof(IAttendeeService))]
    public class AttendeeService : IAttendeeService
    {
        IProfileService _profileService;

        [ImportingConstructor]
        private AttendeeService(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private yieldtome.Objects.Attendee CreateAttendeeObject(Attendee dbAttendee)
        {
            yieldtome.Objects.Attendee attendee = new yieldtome.Objects.Attendee()
            {
                AttendeeID = dbAttendee.AttendeeID,
                Name = dbAttendee.Name,
            };

            if (dbAttendee.Profile != null)
                attendee.Profile = _profileService.GetProfile(dbAttendee.ProfileID);

            return attendee;
        }

        public List<yieldtome.Objects.Attendee> GetAttendees(int eventID)
        {
            Logging.LogWriter.Write(String.Format("Attempting to retrieve Attendees for Event with EventID={0}", eventID));

            List<yieldtome.Objects.Attendee> attendees;
            using (var db = new Database())
            {
                Event dbEvent = db.Events.FirstOrDefault(x => x.EventID == eventID);

                if (dbEvent == null)
                {
                    string message = String.Format("No Event with EventID={0} exists", eventID);
                    throw new ArgumentException(message, "eventID");
                }

                attendees = dbEvent.Attendees
                                .Where(x => x.DeletedTime == null)
                                .Select(x => CreateAttendeeObject(x)).ToList();
            }

            Logging.LogWriter.Write(String.Format("Successfully retrieved {0} Attendees attending Event with EventID={1}", attendees.Count, eventID));
            return attendees;
        }

        public yieldtome.Objects.Attendee GetAttendee(int profileID, int eventID)
        {
            Logging.LogWriter.Write(String.Format("Attempting to retrieve Attendee associated with ProfileID={0} attending EventID={1}", profileID, eventID));

            yieldtome.Objects.Profile profile = _profileService.GetProfile(profileID); // Check that this Profile exists
            if (profile == null) throw new ArgumentException(String.Format("No Profile with ProfileID={0} exists", profileID));

            List<yieldtome.Objects.Attendee> attendees = GetAttendees(eventID); // Get all Attendees for this Event

            yieldtome.Objects.Attendee attendee = attendees.FirstOrDefault(x => x.Profile.ProfileID == profileID);
            Logging.LogWriter.Write(String.Format("Successfully retrieved Attendee associated with ProfileID={0} attending EventID={1}", profileID, eventID));
            return attendee;
        }

        public yieldtome.Objects.Attendee GetAttendee(int attendeeID)
        {
            Logging.LogWriter.Write(String.Format("Attempting to retrieve Attendee with AttendeeID={0}", attendeeID));

            yieldtome.Objects.Attendee attendee;
            using (var db = new Database())
            {
                List<Attendee> dbAttendees = db.Attendees.Where(x => x.DeletedTime == null).ToList();
                Attendee dbAttendee = dbAttendees.FirstOrDefault(x => x.AttendeeID == attendeeID);

                if (dbAttendee == null)
                {
                    Logging.LogWriter.Write(String.Format("No Attendee with AttendeeID={0} exists", attendeeID));
                    return null;
                }

                attendee = CreateAttendeeObject(dbAttendee);
            }

            Logging.LogWriter.Write(String.Format("Successfully retrieved Attendee with AttendeeID={0}", attendeeID));
            return attendee;
        }

        public yieldtome.Objects.Attendee CreateAttendee(int eventID, string name, int profileID)
        {
            Logging.LogWriter.Write("Attempting to create a new Attendee");

            if (name == "") throw new ArgumentNullException("Name is required", "name");

            Attendee dbAttendee;
            using (var db = new Database())
            {
                Profile profile = db.Profiles.FirstOrDefault(x => x.ProfileID == profileID);
                if (profile == null) throw new ArgumentException(String.Format("No Profile with ProfileID={0} exists", profileID));

                Event theEvent = db.Events.FirstOrDefault(x => x.EventID == eventID);
                if (theEvent == null) throw new ArgumentException(String.Format("No Event with EventID={0} exists", eventID));

                dbAttendee = new Attendee()
                {
                    Name = name,
                    Event = theEvent,
                    Profile = profile,
                    CreatedTime = DateTime.Now
                };

                db.Attendees.Add(dbAttendee);
                db.SaveChanges();
            }

            Logging.LogWriter.Write(String.Format("Created a new Attendee with ID {0}", dbAttendee.AttendeeID));
            return CreateAttendeeObject(dbAttendee);
        }

        public void DeleteAttendee(int attendeeID)
        {
            Logging.LogWriter.Write(String.Format("Attempting to delete Attendee with AttendeeID={0}", attendeeID));

            using (var db = new Database())
            {
                Attendee dbAttendee = db.Attendees.FirstOrDefault(x => x.AttendeeID == attendeeID);
                if (dbAttendee == null) throw new ArgumentException(String.Format("No Attendee with AttendeeID={0} exists", attendeeID));

                dbAttendee.DeletedTime = DateTime.Now;
                db.SaveChanges();
            }

            Logging.LogWriter.Write(String.Format("Deleted Attendee with ID {0}", attendeeID));
        }

        public void DeleteAttendees(int eventID)
        {
            Logging.LogWriter.Write(String.Format("Attempting to delete Attendees attending Event with EventID={0}", eventID));

            using (var db = new Database())
            {
                Event dbEvent = db.Events.FirstOrDefault(x => x.EventID == eventID);
                if (dbEvent == null) throw new ArgumentException(String.Format("No Event with EventID={0} exists", eventID));

                foreach (Attendee a in dbEvent.Attendees)
                    if (a.DeletedTime == null) a.DeletedTime = DateTime.Now;

                db.SaveChanges();
            }

            Logging.LogWriter.Write(String.Format("Deleted all Attendees attending Event with EventID={0}", eventID));
        }
    }
}
