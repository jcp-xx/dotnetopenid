﻿//-----------------------------------------------------------------------
// <copyright file="HostProcessedRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// A base class from which identity and non-identity RP requests can derive.
	/// </summary>
	[Serializable]
	internal abstract class HostProcessedRequest : Request, IHostProcessedRequest {
		/// <summary>
		/// The negative assertion to send, if the host site chooses to send it.
		/// </summary>
		private readonly NegativeAssertionResponse negativeResponse;

		/// <summary>
		/// A cache of the result from discovery of the Realm URL.
		/// </summary>
		private RelyingPartyDiscoveryResult? realmDiscoveryResult;

		/// <summary>
		/// Initializes a new instance of the <see cref="HostProcessedRequest"/> class.
		/// </summary>
		/// <param name="provider">The provider that received the request.</param>
		/// <param name="request">The incoming request message.</param>
		protected HostProcessedRequest(OpenIdProvider provider, SignedResponseRequest request)
			: base(request, provider.SecuritySettings) {
			Contract.Requires(provider != null);

			this.negativeResponse = new NegativeAssertionResponse(request, provider.Channel);
		}

		#region IHostProcessedRequest Properties

		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		public ProtocolVersion RelyingPartyVersion {
			get { return Protocol.Lookup(this.RequestMessage.Version).ProtocolVersion; }
		}

		/// <summary>
		/// Gets a value indicating whether the consumer demands an immediate response.
		/// If false, the consumer is willing to wait for the identity provider
		/// to authenticate the user.
		/// </summary>
		public bool Immediate {
			get { return this.RequestMessage.Immediate; }
		}

		/// <summary>
		/// Gets the URL the consumer site claims to use as its 'base' address.
		/// </summary>
		public Realm Realm {
			get { return this.RequestMessage.Realm; }
		}

		/// <summary>
		/// Gets or sets the provider endpoint.
		/// </summary>
		/// <value>
		/// The default value is the URL that the request came in on from the relying party.
		/// </value>
		public abstract Uri ProviderEndpoint { get; set; }

		#endregion

		/// <summary>
		/// Gets a value indicating whether realm discovery been performed.
		/// </summary>
		internal bool HasRealmDiscoveryBeenPerformed {
			get { return this.realmDiscoveryResult.HasValue; }
		}

		/// <summary>
		/// Gets the negative response.
		/// </summary>
		protected NegativeAssertionResponse NegativeResponse {
			get { return this.negativeResponse; }
		}

		/// <summary>
		/// Gets the original request message.
		/// </summary>
		/// <value>This may be null in the case of an unrecognizable message.</value>
		protected new SignedResponseRequest RequestMessage {
			get { return (SignedResponseRequest)base.RequestMessage; }
		}

		#region IHostProcessedRequest Methods

		/// <summary>
		/// Gets a value indicating whether verification of the return URL claimed by the Relying Party
		/// succeeded.
		/// </summary>
		/// <param name="provider">The OpenIdProvider that is performing the RP discovery.</param>
		/// <returns>Result of realm discovery.</returns>
		/// <remarks>
		/// Return URL verification is only attempted if this property is queried.
		/// The result of the verification is cached per request so calling this
		/// property getter multiple times in one request is not a performance hit.
		/// See OpenID Authentication 2.0 spec section 9.2.1.
		/// </remarks>
		public RelyingPartyDiscoveryResult IsReturnUrlDiscoverable(OpenIdProvider provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			if (!this.realmDiscoveryResult.HasValue) {
				this.realmDiscoveryResult = this.IsReturnUrlDiscoverableCore(provider);
			}

			return this.realmDiscoveryResult.Value;
		}

		/// <summary>
		/// Gets a value indicating whether verification of the return URL claimed by the Relying Party
		/// succeeded.
		/// </summary>
		/// <param name="provider">The OpenIdProvider that is performing the RP discovery.</param>
		/// <returns>Result of realm discovery.</returns>
		private RelyingPartyDiscoveryResult IsReturnUrlDiscoverableCore(OpenIdProvider provider) {
			Contract.Requires(provider != null);

			ErrorUtilities.VerifyInternal(this.Realm != null, "Realm should have been read or derived by now.");
			try {
				if (this.SecuritySettings.RequireSsl && this.Realm.Scheme != Uri.UriSchemeHttps) {
					Logger.OpenId.WarnFormat("RP discovery failed because RequireSsl is true and RP discovery would begin at insecure URL {0}.", this.Realm);
					return RelyingPartyDiscoveryResult.NoServiceDocument;
				}

				var returnToEndpoints = this.Realm.Discover(provider.Channel.WebRequestHandler, false);
				if (returnToEndpoints == null) {
					return RelyingPartyDiscoveryResult.NoServiceDocument;
				}

				foreach (var returnUrl in returnToEndpoints) {
					Realm discoveredReturnToUrl = returnUrl.ReturnToEndpoint;

					// The spec requires that the return_to URLs given in an RPs XRDS doc
					// do not contain wildcards.
					if (discoveredReturnToUrl.DomainWildcard) {
						Logger.Yadis.WarnFormat("Realm {0} contained return_to URL {1} which contains a wildcard, which is not allowed.", Realm, discoveredReturnToUrl);
						continue;
					}

					// Use the same rules as return_to/realm matching to check whether this
					// URL fits the return_to URL we were given.
					if (discoveredReturnToUrl.Contains(this.RequestMessage.ReturnTo)) {
						// no need to keep looking after we find a match
						return RelyingPartyDiscoveryResult.Success;
					}
				}
			} catch (ProtocolException ex) {
				// Don't do anything else.  We quietly fail at return_to verification and return false.
				Logger.Yadis.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
				return RelyingPartyDiscoveryResult.NoServiceDocument;
			} catch (WebException ex) {
				// Don't do anything else.  We quietly fail at return_to verification and return false.
				Logger.Yadis.InfoFormat("Relying party discovery at URL {0} failed.  {1}", Realm, ex);
				return RelyingPartyDiscoveryResult.NoServiceDocument;
			}

			return RelyingPartyDiscoveryResult.NoMatchingReturnTo;
		}

		#endregion
	}
}
