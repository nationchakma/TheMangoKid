﻿using ServiceLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace WebApp.Controllers
{
    [Authorize]
    public class SoloController : Controller
    {
        GeneralLogics logic;
        BusinessLogics businessLogics;
        // GET: Solo
        [HttpGet]
        public ActionResult AddTrackToAlbum(Guid albumId)
        {
            businessLogics = new BusinessLogics();
            string userEmail = Session["LoginEmail"].ToString();
            if (userEmail != null && businessLogics.IsAccountContainsThisAlbum(userEmail, albumId))
            {
                if (!businessLogics.IsAlbumExpired(albumId))
                {
                    ViewBag.AlbumId = albumId;
                    ViewBag.Title = "Add Track";
                    return View("AddTrackAlbum");
                }
                else
                {
                    TempData["ErrorMsg"] = "You are trying to add an track to an album that is already expired";
                }
            }
            else
            {
                TempData["ErrorMsg"] = "You are trying to add an track to an album that doesn't belongs to you.";
            }
            return RedirectToAction("Index", "UserProfile");
        }

        [HttpGet]
        public ActionResult AddTrackToEp(Guid epId)
        {
            businessLogics = new BusinessLogics();
            string userEmail = Session["LoginEmail"].ToString();
            if (userEmail != null && businessLogics.IsAccountContainsThisEp(userEmail, epId))
            {
                if (!businessLogics.IsEpExpired(epId))
                {
                    ViewBag.EpId = epId;
                    ViewBag.Title = "Add Track";
                    return View("AddTrackEp");
                }
                else
                {
                    TempData["ErrorMsg"] = "You are trying to add an track to an Ep that is already expired";
                }
                   
            }
            else
            {
                TempData["ErrorMsg"] = "You are trying to add an track to an Ep that doesn't belongs to you.";
            }
            return RedirectToAction("Index", "UserProfile");
        }

        [HttpGet]
        public ActionResult AddTrackToSolo()
        {
            businessLogics = new BusinessLogics();
            string userEmail = Session["LoginEmail"].ToString();
            //Get first unused purchaseId for the user to create solo
            Guid? purchaseId = businessLogics.GetFirstPurchaseIdForSoloOf(userEmail);
            if (purchaseId != null)
            {
                if (userEmail != null && businessLogics.IsAccountContainsThisPurchase(userEmail, purchaseId))
                {
                    if (!businessLogics.IsPurchaseExpired(purchaseId))
                    {
                        ViewBag.PurchaseId = purchaseId;
                        ViewBag.Title = "Add Track";
                        return View("AddTrackSolo");
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Your purchase has expired";
                    }

                }
                else
                {
                    TempData["ErrorMsg"] = "You are trying to add an track to an Ep that doesn't belongs to you.";
                }
            }
            else
            {
                TempData["ErrorMsg"] = "You don't have any purchase record left to submit a solo track.";
            }
            return RedirectToAction("Index", "UserProfile");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTrackToAlbum(Guid albumId, string TrackTitle, string ArtistName, string ArtistAlreadyInSpotify, string ArtistSpotifyUrl, DateTime ReleaseDate, string Genre, string CopyrightClaimerName, string AuthorName, string ComposerName, string ArrangerName, string ProducerName, string AlreadyHaveAnISRC, string ISRC_Number, string PriceTier, string ExplicitContent, string IsTrackInstrumental, string LyricsLanguage, string TrackZipFileLink, string ArtWork_Link)
        {
            List<string> mandetoryFieldsInput = new List<string> { TrackTitle, ArtistName, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, PriceTier, TrackZipFileLink, ArtWork_Link };
            List<string> mandetoryBoolFields = new List<string> { ArtistAlreadyInSpotify, AlreadyHaveAnISRC, ExplicitContent, IsTrackInstrumental };

            logic = new GeneralLogics();

            if (!logic.ContainsAnyNullorWhiteSpace(mandetoryFieldsInput))
            {
                if (!logic.ContainsAnyNullorWhiteSpace(mandetoryBoolFields))
                {
                    bool isArtistOnSpotify = false, isTrackHasISRC = false, isTrackHasExplicitContent = false, isTrackInstrumental = false;
                    if (ArtistAlreadyInSpotify == "yes")
                    {
                        isArtistOnSpotify = true;
                    }
                    if (AlreadyHaveAnISRC == "yes")
                    {
                        isTrackHasISRC = true;
                    }
                    if (ExplicitContent == "yes")
                    {
                        isTrackHasExplicitContent = true;
                    }
                    if (IsTrackInstrumental == "yes")
                    {
                        isTrackInstrumental = true;
                    }

                    if ((isArtistOnSpotify && !String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())) || (isArtistOnSpotify==false && String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())))
                    {
                        if ((isTrackHasISRC && !String.IsNullOrWhiteSpace(ISRC_Number.Trim())) || (isTrackHasISRC==false && String.IsNullOrWhiteSpace(ISRC_Number.Trim())))
                        {
                            if ((isTrackInstrumental && String.IsNullOrWhiteSpace(LyricsLanguage.Trim()))||(isTrackInstrumental==false && !String.IsNullOrWhiteSpace(LyricsLanguage.Trim())))
                            {
                                if (ReleaseDate != null && ReleaseDate > logic.CurrentIndianTime())
                                {
                                    //Code to add the song to the album
                                    businessLogics = new BusinessLogics();
                                    var result = businessLogics.CreateNewTrackForAlbum(albumId, TrackTitle, ArtistName, isArtistOnSpotify, ArtistSpotifyUrl, ReleaseDate, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, isTrackHasISRC, ISRC_Number, Convert.ToInt32(PriceTier), isTrackHasExplicitContent, isTrackInstrumental, LyricsLanguage, TrackZipFileLink, ArtWork_Link);
                                    if (result == 1)
                                    {
                                        return RedirectToAction("ShowIndividualAlbumSongs", "Album", new { albumId = albumId });
                                    }
                                    else if (result == 7)
                                    {
                                        TempData["ErrorMsg"] = "Your purchase has expired. you can't add the track to the album.";
                                    }
                                    else if (result == 8)
                                    {
                                        TempData["ErrorMsg"] = "You can't add the track as the album is full.";
                                    }
                                    else
                                    {
                                        TempData["ErrorMsg"] = "Internal Error occured";
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Provide a valid Date to release your track";
                                }
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "If it's an instrumental track then leave the Lyrics Language field empty";
                            }
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "If you have ISRC number for the track then select yes and provide the number. Otherwise select no and leave the field empty.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "If artist is already on spotify then select yes and provide the link of the artist. Otherwise select no and leave the field empty.";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Select proper options from dropdowns";
                }

            }
            else
            {
                TempData["ErrorMsg"] = "Mandetory Fields can't be left empty";
            }
            return RedirectToAction("AddTrackToAlbum","Solo",new { albumId =albumId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTrackToEp(Guid epId, string TrackTitle, string ArtistName, string ArtistAlreadyInSpotify, string ArtistSpotifyUrl, DateTime ReleaseDate, string Genre, string CopyrightClaimerName, string AuthorName, string ComposerName, string ArrangerName, string ProducerName, string AlreadyHaveAnISRC, string ISRC_Number, string PriceTier, string ExplicitContent, string IsTrackInstrumental, string LyricsLanguage, string TrackZipFileLink, string ArtWork_Link)
        {
            List<string> mandetoryFieldsInput = new List<string> { TrackTitle, ArtistName, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, PriceTier, TrackZipFileLink, ArtWork_Link };
            List<string> mandetoryBoolFields = new List<string> { ArtistAlreadyInSpotify, AlreadyHaveAnISRC, ExplicitContent, IsTrackInstrumental };

            logic = new GeneralLogics();

            if (!logic.ContainsAnyNullorWhiteSpace(mandetoryFieldsInput))
            {
                if (!logic.ContainsAnyNullorWhiteSpace(mandetoryBoolFields))
                {
                    bool isArtistOnSpotify = false, isTrackHasISRC = false, isTrackHasExplicitContent = false, isTrackInstrumental = false;
                    if (ArtistAlreadyInSpotify == "yes")
                    {
                        isArtistOnSpotify = true;
                    }
                    if (AlreadyHaveAnISRC == "yes")
                    {
                        isTrackHasISRC = true;
                    }
                    if (ExplicitContent == "yes")
                    {
                        isTrackHasExplicitContent = true;
                    }
                    if (IsTrackInstrumental == "yes")
                    {
                        isTrackInstrumental = true;
                    }

                    if ((isArtistOnSpotify && !String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())) || (isArtistOnSpotify == false && String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())))
                    {
                        if ((isTrackHasISRC && !String.IsNullOrWhiteSpace(ISRC_Number.Trim())) || (isTrackHasISRC == false && String.IsNullOrWhiteSpace(ISRC_Number.Trim())))
                        {
                            if ((isTrackInstrumental && String.IsNullOrWhiteSpace(LyricsLanguage.Trim())) || (isTrackInstrumental == false && !String.IsNullOrWhiteSpace(LyricsLanguage.Trim())))
                            {
                                if (ReleaseDate != null && ReleaseDate > logic.CurrentIndianTime())
                                {
                                    //Code to add the song to the Ep
                                    businessLogics = new BusinessLogics();
                                    var result = businessLogics.CreateNewTrackForEp(epId, TrackTitle, ArtistName, isArtistOnSpotify, ArtistSpotifyUrl, ReleaseDate, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, isTrackHasISRC, ISRC_Number, Convert.ToInt32(PriceTier), isTrackHasExplicitContent, isTrackInstrumental, LyricsLanguage, TrackZipFileLink, ArtWork_Link);
                                    if (result == 1)
                                    {
                                        return RedirectToAction("ShowIndividualEpSongs", "ExtendedPlay", new { epId = epId });
                                    }
                                    else if (result == 7)
                                    {
                                        TempData["ErrorMsg"] = "Your purchase has expired. you can't add the track to the Ep.";
                                    }
                                    else if (result == 8)
                                    {
                                        TempData["ErrorMsg"] = "You can't add the track as the Ep is full.";
                                    }
                                    else
                                    {
                                        TempData["ErrorMsg"] = "Internal Error occured";
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Provide a valid Date to release your track";
                                }
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "If it's an instrumental track then leave the Lyrics Language field empty";
                            }
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "If you have ISRC number for the track then select yes and provide the number. Otherwise select no and leave the field empty.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "If artist is already on spotify then select yes and provide the link of the artist. Otherwise select no and leave the field empty.";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Select proper options from dropdowns";
                }

            }
            else
            {
                TempData["ErrorMsg"] = "Mandetory Fields can't be left empty";
            }
            return RedirectToAction("AddTrackToEp", "Solo", new { epId = epId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTrackToSolo(Guid purchaseId, string TrackTitle, string ArtistName, string ArtistAlreadyInSpotify, string ArtistSpotifyUrl, DateTime ReleaseDate, string Genre, string CopyrightClaimerName, string AuthorName, string ComposerName, string ArrangerName, string ProducerName, string AlreadyHaveAnISRC, string ISRC_Number, string PriceTier, string ExplicitContent, string IsTrackInstrumental, string LyricsLanguage, string TrackZipFileLink, string ArtWork_Link)
        {
            List<string> mandetoryFieldsInput = new List<string> { TrackTitle, ArtistName, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, PriceTier, TrackZipFileLink, ArtWork_Link };
            List<string> mandetoryBoolFields = new List<string> { ArtistAlreadyInSpotify, AlreadyHaveAnISRC, ExplicitContent, IsTrackInstrumental };

            logic = new GeneralLogics();

            if (!logic.ContainsAnyNullorWhiteSpace(mandetoryFieldsInput))
            {
                if (!logic.ContainsAnyNullorWhiteSpace(mandetoryBoolFields))
                {
                    bool isArtistOnSpotify = false, isTrackHasISRC = false, isTrackHasExplicitContent = false, isTrackInstrumental = false;
                    if (ArtistAlreadyInSpotify == "yes")
                    {
                        isArtistOnSpotify = true;
                    }
                    if (AlreadyHaveAnISRC == "yes")
                    {
                        isTrackHasISRC = true;
                    }
                    if (ExplicitContent == "yes")
                    {
                        isTrackHasExplicitContent = true;
                    }
                    if (IsTrackInstrumental == "yes")
                    {
                        isTrackInstrumental = true;
                    }

                    if ((isArtistOnSpotify && !String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())) || (isArtistOnSpotify == false && String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())))
                    {
                        if ((isTrackHasISRC && !String.IsNullOrWhiteSpace(ISRC_Number.Trim())) || (isTrackHasISRC == false && String.IsNullOrWhiteSpace(ISRC_Number.Trim())))
                        {
                            if ((isTrackInstrumental && String.IsNullOrWhiteSpace(LyricsLanguage.Trim())) || (isTrackInstrumental == false && !String.IsNullOrWhiteSpace(LyricsLanguage.Trim())))
                            {
                                if (ReleaseDate != null && ReleaseDate > logic.CurrentIndianTime())
                                {
                                    //Code to add the song to the Ep
                                    businessLogics = new BusinessLogics();
                                    var result = businessLogics.CreateNewTrackForSolo(purchaseId, TrackTitle, ArtistName, isArtistOnSpotify, ArtistSpotifyUrl, ReleaseDate, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, isTrackHasISRC, ISRC_Number, Convert.ToInt32(PriceTier), isTrackHasExplicitContent, isTrackInstrumental, LyricsLanguage, TrackZipFileLink, ArtWork_Link);
                                    if (result == 1)
                                    {
                                        return RedirectToAction("Index", "UserProfile");
                                    }
                                    else if (result == 7)
                                    {
                                        TempData["ErrorMsg"] = "Your purchase has expired. you can't add the track to the Ep.";
                                    }
                                    else if (result == 8)
                                    {
                                        TempData["ErrorMsg"] = "You can't add the track as the Ep is full.";
                                    }
                                    else
                                    {
                                        TempData["ErrorMsg"] = "Internal Error occured";
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Provide a valid Date to release your track";
                                }
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "If it's an instrumental track then leave the Lyrics Language field empty";
                            }
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "If you have ISRC number for the track then select yes and provide the number. Otherwise select no and leave the field empty.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "If artist is already on spotify then select yes and provide the link of the artist. Otherwise select no and leave the field empty.";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Select proper options from dropdowns";
                }

            }
            else
            {
                TempData["ErrorMsg"] = "Mandetory Fields can't be left empty";
            }
            return RedirectToAction("AddTrackToSolo", "Solo", new { purchaseId = purchaseId });
        }


        [HttpGet]
        public ActionResult EditTrackDetailsForAlbum(Guid albumId, Guid trackId)
        {
            businessLogics = new BusinessLogics();
            logic = new GeneralLogics();
            string userEmail = Session["LoginEmail"].ToString();
            if (userEmail != null && businessLogics.IsAccountContainsThisAlbum(userEmail, albumId))
            {
                var albumDetails = businessLogics.GetAlbumDetail(albumId, trackId);
                if (albumDetails != null)
                {
                    if (albumDetails.StoreSubmissionStatus == 0)
                    {
                        var albumPurchase = businessLogics.GetAlbumById((Guid)albumDetails.Album_Id);
                        if (albumPurchase.PurchaseRecord.Usage_Exp_Date > logic.CurrentIndianTime())
                        {
                            var trackDetail = businessLogics.GetTrackById(trackId);
                            if (trackDetail != null)
                            {
                                ViewBag.Title = "Edit Track";
                                ViewBag.TrackDetail = trackDetail;
                                ViewBag.TrackId = trackDetail.Id;
                                return View("EditTrack");
                            }
                            else
                            {
                                TempData["ErrorMsg"] = "Error while fetching track details";
                            }

                        }
                        else
                        {
                            TempData["ErrorMsg"] = "Your purchase has expired. you can't modify the track";
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "The track is already submitted to store. You can't edit this track";
                    }
                }
                else
                {
                    TempData["ErrorMsg"] = "Track is not valid";
                }
            }
            else
            {
                TempData["ErrorMsg"] = "You are trying to modify a track details that doesn't belongs to you";
            }
            return RedirectToAction("ShowIndividualAlbumSongs", "Album", new { albumId = albumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTrackDetails(Guid trackId, string TrackTitle, string ArtistName, string ArtistAlreadyInSpotify, string ArtistSpotifyUrl, DateTime ReleaseDate, string Genre, string CopyrightClaimerName, string AuthorName, string ComposerName, string ArrangerName, string ProducerName, string AlreadyHaveAnISRC, string ISRC_Number, string PriceTier, string ExplicitContent, string IsTrackInstrumental, string LyricsLanguage, string TrackZipFileLink, string ArtWork_Link)
        {
            ViewBag.TrackId = trackId;
            List<string> mandetoryFieldsInput = new List<string> { TrackTitle, ArtistName, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, PriceTier, TrackZipFileLink, ArtWork_Link };
            List<string> mandetoryBoolFields = new List<string> { ArtistAlreadyInSpotify, AlreadyHaveAnISRC, ExplicitContent, IsTrackInstrumental };

            logic = new GeneralLogics();

            if (!logic.ContainsAnyNullorWhiteSpace(mandetoryFieldsInput))
            {
                if (!logic.ContainsAnyNullorWhiteSpace(mandetoryBoolFields))
                {
                    bool isArtistOnSpotify = false, isTrackHasISRC = false, isTrackHasExplicitContent = false, isTrackInstrumental = false;
                    if (ArtistAlreadyInSpotify == "yes")
                    {
                        isArtistOnSpotify = true;
                    }
                    if (AlreadyHaveAnISRC == "yes")
                    {
                        isTrackHasISRC = true;
                    }
                    if (ExplicitContent == "yes")
                    {
                        isTrackHasExplicitContent = true;
                    }
                    if (IsTrackInstrumental == "yes")
                    {
                        isTrackInstrumental = true;
                    }

                    if ((isArtistOnSpotify && !String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())) || (isArtistOnSpotify == false && String.IsNullOrWhiteSpace(ArtistSpotifyUrl.Trim())))
                    {
                        if ((isTrackHasISRC && !String.IsNullOrWhiteSpace(ISRC_Number.Trim())) || (isTrackHasISRC == false && String.IsNullOrWhiteSpace(ISRC_Number.Trim())))
                        {
                            if ((isTrackInstrumental && String.IsNullOrWhiteSpace(LyricsLanguage.Trim())) || (isTrackInstrumental == false && !String.IsNullOrWhiteSpace(LyricsLanguage.Trim())))
                            {
                                if (ReleaseDate != null && ReleaseDate > logic.CurrentIndianTime())
                                {
                                    //Code to add the song to the album
                                    businessLogics = new BusinessLogics();
                                    var result = businessLogics.EditTrack(trackId, TrackTitle, ArtistName, isArtistOnSpotify, ArtistSpotifyUrl, ReleaseDate, Genre, CopyrightClaimerName, AuthorName, ComposerName, ArrangerName, ProducerName, isTrackHasISRC, ISRC_Number, Convert.ToInt32(PriceTier), isTrackHasExplicitContent, isTrackInstrumental, LyricsLanguage, TrackZipFileLink, ArtWork_Link);
                                    if (result == 1)
                                    {
                                        return RedirectToAction("Index", "UserProfile");
                                    }
                                    else if (result == 7)
                                    {
                                        ViewBag.ErrorMsg = "Your purchase has expired. you can't add the track to the album.";
                                    }
                                    else if (result == 8)
                                    {
                                        ViewBag.ErrorMsg = "You can't add the track as the album is full.";
                                    }
                                    else
                                    {
                                        ViewBag.ErrorMsg = "Internal Error occured";
                                    }
                                }
                                else
                                {
                                    ViewBag.ErrorMsg = "Provide a valid Date to release your track";
                                }
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "If it's an instrumental track then leave the Lyrics Language field empty";
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMsg = "If you have ISRC number for the track then select yes and provide the number. Otherwise select no and leave the field empty.";
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "If artist is already on spotify then select yes and provide the link of the artist. Otherwise select no and leave the field empty.";
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = "Select proper options from dropdowns";
                }

            }
            else
            {
                ViewBag.ErrorMsg = "Mandetory Fields can't be left empty";
            }
            return View("EditTrack");
        }

        [HttpGet]
        public ActionResult RemoveTrackFromAlbum(Guid albumId, Guid trackId)
        {
            return View();
        }

        [HttpGet]
        public ActionResult ShowSoloTrackDetail(Guid trackId, int? StoreSubmissionStatus)
        {
            string userEmail = Session["LoginEmail"].ToString();
            if (userEmail != null)
            {
                businessLogics = new BusinessLogics();
                if (businessLogics.IsAccountContainsThisSolo(userEmail, trackId)||User.IsInRole("superadmin")|| User.IsInRole("admin"))
                {
                    ViewBag.Title = "Solo Details";

                    var soloObject = businessLogics.GetTrackById(trackId);
                    if (soloObject != null)
                    {
                        ViewBag.TrackDetail = soloObject;
                        ViewBag.StoreSubmissionStatus = StoreSubmissionStatus;
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "No track found with the Id provided";
                    }
                    return View("ShowTrackDetail");
                }
                else
                {
                    TempData["ErrorMsg"] = "You are trying to view a track detail that does not belongs to you";
                    return RedirectToAction("Index", "UserProfile");
                }
            }
            else
            {
                return RedirectToAction("Index", "UserProfile");
            }
        }

        [HttpGet]
        public ActionResult ShowEpTrackDetail(Guid epId, Guid trackId, int? StoreSubmissionStatus)
        {
            string userEmail = Session["LoginEmail"].ToString();
            if (userEmail != null)
            {
                businessLogics = new BusinessLogics();
                var singlesOfThisEp = businessLogics.GetTrackDetailsOfEp(epId);
                if ((businessLogics.IsAccountContainsThisEp(userEmail, epId) &&
                    singlesOfThisEp.Any(solo=>solo.Id==trackId)) || 
                    User.IsInRole("superadmin") || 
                    User.IsInRole("admin"))
                {
                    ViewBag.TrackDetail = singlesOfThisEp.Where(track=>track.Id==trackId).SingleOrDefault();
                    ViewBag.Category = "Extended Play";
                    ViewBag.EpDetail = businessLogics.GetEpById(epId);
                    ViewBag.StoreSubmissionStatus = StoreSubmissionStatus;
                    return View("ShowTrackDetail");
                }
                else
                {
                    TempData["ErrorMsg"] = "Something suspecious occured";
                    return RedirectToAction("Index", "UserProfile");
                }
            }
            else
            {
                return RedirectToAction("Index", "UserProfile");
            }
        }

        [HttpGet]
        public ActionResult ShowAlbumTrackDetail(Guid albumId, Guid trackId, int? StoreSubmissionStatus)
        {
            if (Session["LoginEmail"] != null)
            {
                string userEmail = Session["LoginEmail"].ToString();
                businessLogics = new BusinessLogics();
                var singlesOfThisAlbum = businessLogics.GetTrackDetailsOfAlbum(albumId);
                if ((businessLogics.IsAccountContainsThisAlbum(userEmail, albumId) &&
                    singlesOfThisAlbum.Any(solo => solo.Id == trackId)) ||
                    User.IsInRole("superadmin") ||
                    User.IsInRole("admin"))
                {
                    ViewBag.TrackDetail = singlesOfThisAlbum.Where(track => track.Id == trackId).SingleOrDefault();
                    ViewBag.Category = "Album";
                    ViewBag.AlbumDetail = businessLogics.GetAlbumById(albumId);
                    ViewBag.StoreSubmissionStatus = StoreSubmissionStatus;
                    return View("ShowTrackDetail");
                }
                else
                {
                    TempData["ErrorMsg"] = "Something suspecious occured";
                    return RedirectToAction("Index", "UserProfile");
                }
            }
            else
            {
                return RedirectToAction("Index", "UserProfile");
            }
        }

        [HttpPost]
        [Authorize(Roles ="superadmin,admin")]
        public ActionResult AssignISRCnumber(Guid trackId, string trackISRCnumber)
        {
            businessLogics = new BusinessLogics();
            if (businessLogics.UpdateISRCNumberOfTrack(trackId, trackISRCnumber) == 1)
            {
                TempData["Msg"] = "Successfully ISRC number assigned for the track";
            }
            else
            {
                TempData["ErrorMsg"] = "Failed to assign ISRC number for the track. Error occured.";
            }

            if (User.IsInRole("superadmin"))
            {
                return RedirectToAction("Index", "SuperAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            
        }

        [HttpGet]
        [Authorize(Roles ="superadmin,admin")]
        public ActionResult UpdateStatusAlbumTrack(Guid albumId, Guid trackId, int storeSubmissionStatus)
        {
            businessLogics = new BusinessLogics();
            if (businessLogics.UpdateStoreSubmissionStatusForAlbumTrack(albumId, trackId, storeSubmissionStatus) != 1)
            {
                TempData["ErrorMsg"] = "Status update failed. Try again.";
            }

            if (User.IsInRole("superadmin"))
            {
                return RedirectToAction("Index", "SuperAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpGet]
        [Authorize(Roles = "superadmin,admin")]
        public ActionResult UpdateStatusEpTrack(Guid epId, Guid trackId, int storeSubmissionStatus)
        {
            businessLogics = new BusinessLogics();
            if (businessLogics.UpdateStoreSubmissionStatusForEpTrack(epId, trackId, storeSubmissionStatus) != 1)
            {
                TempData["ErrorMsg"] = "Status update failed. Try again.";
            }
            if (User.IsInRole("superadmin"))
            {
                return RedirectToAction("Index", "SuperAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpGet]
        [Authorize(Roles = "superadmin,admin")]
        public ActionResult UpdateStatusSoloTrack(Guid trackId, int storeSubmissionStatus)
        {
            businessLogics = new BusinessLogics();
            if (businessLogics.UpdateStoreSubmissionStatusForSoloTrack(trackId, storeSubmissionStatus) != 1)
            {
                TempData["ErrorMsg"] = "Status update failed. Try again.";
            }
            if (User.IsInRole("superadmin"))
            {
                return RedirectToAction("Index", "SuperAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }
    }
}