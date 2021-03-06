﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using yieldtome.Interfaces;
using yieldtome.Objects;
using System.Collections.Generic;

namespace yieldtome.API.Tests
{
    [TestClass]
    public class AttendeeServiceTests
    {
        IAttendeeService _service;
        List<int> _deletions = new List<int>();

        [TestInitialize()]
        public void Initialize()
        {
            if (!Extensibility.IsContainerRunning)
                Extensibility.StartContainer();

            _service = Extensibility.Container.GetExportedValue<IAttendeeService>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetAttendees_EventNotFound()
        {
            List<Attendee> attendees = _service.GetAttendees(-1);
        }        
        
        [TestMethod]
        public void GetAttendees_Success()
        {
            List<Attendee> attendees = _service.GetAttendees(1);
            Assert.IsInstanceOfType(attendees, typeof(List<Attendee>));
            CollectionAssert.AllItemsAreNotNull(attendees);
            CollectionAssert.AllItemsAreUnique(attendees);
        }

        [TestMethod]
        public void GetAttendee_AttendeeNotFound()
        {
            Attendee attendee = _service.GetAttendee(-1);
            Assert.IsNull(attendee);
        }

        [TestMethod]
        public void GetAttendee_Success()
        {
            Attendee attendee = _service.GetAttendee(1);
            Assert.IsNotNull(attendee);
            Assert.IsInstanceOfType(attendee, typeof(Attendee));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetAttendeeByProfile_InvalidProfile()
        {
            Attendee attendee = _service.GetAttendee(-1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetAttendeeByProfile_InvalidEvent()
        {
            Attendee attendee = _service.GetAttendee(1, -1);
        }

        [TestMethod]
        public void GetAttendeeByProfile_NotAttending()
        {
            Attendee attendee = _service.GetAttendee(1, 2);
            Assert.IsNull(attendee);
        }

        [TestMethod]
        public void GetAttendeeByProfile_Success()
        {
            Attendee attendee = _service.GetAttendee(1, 1);
            Assert.IsNotNull(attendee);
            Assert.IsInstanceOfType(attendee, typeof(Attendee));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateAttendee_InvalidEventID()
        {
            Attendee attendee = _service.CreateAttendee(-1, "New Attendee", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateAttendee_MissingName()
        {
            Attendee attendee = _service.CreateAttendee(1, "", 1);
        }

        [TestMethod]
        public void CreateAttendee_Success()
        {
            Attendee attendee = _service.CreateAttendee(1, "New Attendee", 1);

            _deletions.Add(attendee.AttendeeID); // Delete this in the tear down

            Assert.IsInstanceOfType(attendee, typeof(Attendee));
            Assert.AreEqual("New Attendee", attendee.Name);
            Assert.AreEqual(1, attendee.Profile.ProfileID);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteAttendee_InvalidAttendeeID()
        {
            _service.DeleteAttendee(-1);
        }

        [TestMethod]
        public void DeleteAttendee_Success()
        {
            Attendee attendeeToDelete = _service.CreateAttendee(1, String.Format("Attendee to delete {0}", DateTime.Now.Ticks), 1);
            _service.DeleteAttendee(attendeeToDelete.AttendeeID);

            Attendee deletedAttendee = null;
            try { deletedAttendee = _service.GetAttendee(attendeeToDelete.AttendeeID); }
            catch (ArgumentException) { }

            Assert.IsNull(deletedAttendee);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeleteAttendees_InvalidEventID()
        {
            _service.DeleteAttendees(-1);
        }

        [TestMethod]
        public void DeleteAttendees_Success()
        {
            Attendee attendeeToDelete = _service.CreateAttendee(1, String.Format("Attendee to delete {0}", DateTime.Now.Ticks), 1);
            _service.DeleteAttendee(attendeeToDelete.AttendeeID);

            Attendee deletedAttendee = null;
            try { deletedAttendee = _service.GetAttendee(attendeeToDelete.AttendeeID); }
            catch (ArgumentException) { }

            Assert.IsNull(deletedAttendee);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            foreach (int i in _deletions)
                _service.DeleteAttendee(i);
        }
    }
}
