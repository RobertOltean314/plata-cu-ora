using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using FirebaseAdmin.Auth;
using System.Net.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using PlataCuOra.Server.Domain.DTO;
using PlataCuOra.Server.Domain;


namespace PlataCuOra.Server.Data
{
	public class AppDbContext
	{
		public readonly FirestoreDb _firestoreDb;
		public readonly FirebaseAuth _auth;

		public AppDbContext()
		{
			_firestoreDb = FirestoreDb.Create("PlataCuOra", new FirestoreClientBuilder
			{
				Credential = GoogleCredential.FromFile("./firebaseKey.json")
			}.Build());
		}

	}
}
