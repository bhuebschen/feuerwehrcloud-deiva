#region Header
// Copyright (c) 2013 Hans Wolff
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System.Linq;


#endregion

using SMTPd.Logging;
using System;

namespace SMTPd.Smtp
{
    class SmtpSessionInfoResponder : SmtpCommandParser
    {

		public ISmtpSessionInfo SessionInfo { get; private set; }
        
        private readonly ISmtpResponderFactory _responderFactory;

        public SmtpSessionInfoResponder(ISmtpResponderFactory responderFactory, ISmtpSessionInfo sessionInfo)
        {
            if (responderFactory == null) throw new ArgumentNullException("responderFactory");
            if (sessionInfo == null) throw new ArgumentNullException("sessionInfo");

            SessionInfo = sessionInfo;
            _responderFactory = responderFactory;
        }

		protected override SmtpResponse ProcessCommandTLSStart (string name, string arguments)
		{
			//SessionInfo.Reset ();

			return  SmtpResponse.StartTLS;
			//throw new NotImplementedException ();
		}

        protected override SmtpResponse ProcessCommandDataStart(string name, string arguments)
        {
            // ReSharper disable PossibleUnintendedReferenceComparison
            var notIdentified = CreateResponseIfNotIdentified();
            if (notIdentified.HasValue) return notIdentified;

            var hasNotMailFrom = CreateResponseIfHasNotMailFrom();
            if (hasNotMailFrom.HasValue) return hasNotMailFrom;

            var hasNoRecipients = CreateResponseIfHasNoRecipients();
            if (hasNoRecipients.HasValue) return hasNoRecipients;
            // ReSharper restore PossibleUnintendedReferenceComparison

            var response = _responderFactory.DataResponder.DataStart(SessionInfo);

            if (response.Success)
            {
                InDataMode = true;
                SessionInfo.HasData = true;
            }

            return response;
        }

        protected override SmtpResponse ProcessCommandEhlo(string name, string arguments)
        {
            if (String.IsNullOrWhiteSpace(arguments))
            {
				return new SmtpResponse(501, "EHLO Missing domain address. (#5.5.0)");
            }

            var identification = new SmtpIdentification(SmtpIdentificationMode.EHLO, arguments);
            var response = _responderFactory.IdentificationResponder.VerifyIdentification(SessionInfo, identification);

            if (response.Success)
            {
                SessionInfo.Identification = identification;
            }

            return response;
        }

        protected override SmtpResponse ProcessCommandHelo(string name, string arguments)
        {
            if (String.IsNullOrWhiteSpace(arguments))
            {
				return new SmtpResponse(501, "HELO Missing domain address. (#5.5.0)");
            }

            var identification = new SmtpIdentification(SmtpIdentificationMode.HELO, arguments);
            var response = _responderFactory.IdentificationResponder.VerifyIdentification(SessionInfo, identification);

            if (response.Success)
            {
                SessionInfo.Identification = identification;
            }

            return response;
        }

        protected override SmtpResponse ProcessCommandMailFrom(string name, string arguments)
        {
            var notIdentified = CreateResponseIfNotIdentified();
            if (notIdentified.HasValue) return notIdentified;

            var mailFrom = arguments.Trim();
            MailAddressWithParameters mailAddressWithParameters;
            try { mailAddressWithParameters = MailAddressWithParameters.Parse(mailFrom); }
            catch (FormatException)
            {
                return SmtpResponses.SyntaxError;
            }

            var response = _responderFactory.MailFromResponder.VerifyMailFrom(SessionInfo, mailAddressWithParameters);
            if (response.Success)
            {
                SessionInfo.MailFrom = mailAddressWithParameters;
            }

            return response;
        }

        protected override SmtpResponse ProcessCommandNoop(string name, string arguments)
        {
            return SmtpResponses.OK;
        }

        protected override SmtpResponse ProcessCommandQuit(string name, string arguments)
        {
            return SmtpResponses.Disconnect;
        }

        protected override SmtpResponse ProcessCommandRcptTo(string name, string arguments)
        {
            var notIdentified = CreateResponseIfNotIdentified();
            if (notIdentified.HasValue) return notIdentified;

            var hasNotMailFrom = CreateResponseIfHasNotMailFrom();
            if (hasNotMailFrom.HasValue) return hasNotMailFrom;

            var recipient = arguments.Trim();
            MailAddressWithParameters mailAddressWithParameters;
            try { mailAddressWithParameters = MailAddressWithParameters.Parse(recipient); }
            catch (FormatException)
            {
                return SmtpResponses.SyntaxError;
            }

            var response = _responderFactory.RecipientToResponder.VerifyRecipientTo(SessionInfo, mailAddressWithParameters);
            if (response.Success)
            {
                SessionInfo.Recipients.Add(mailAddressWithParameters);
            }

            return response;
        }

        protected override SmtpResponse ProcessCommandRset(string name, string arguments)
        {
            var response = _responderFactory.ResetResponder.Reset(SessionInfo);
            if (response.Success)
            {
                SessionInfo.Reset();
                InDataMode = false;
            }
            return response;
        }

		protected override SmtpResponse ProcessCommandLIC (string name, string arguments)
		{
			return new SmtpResponse (250, "Licensed for " ); //+ System.IO.File.ReadAllLines("")[0]
		}

		protected override SmtpResponse ProcessCommandXVRB (string name, string arguments)
		{
			return SmtpResponse.NotImplemented;
		}

		protected override SmtpResponse ProcessCommandSAML (string name, string arguments)
		{
			ProcessCommandRcptTo(name, arguments);
			return SmtpResponse.NotImplemented;
		}

		protected override SmtpResponse ProcessCommandSOML (string name, string arguments)
		{
			ProcessCommandRcptTo(name, arguments);
			return SmtpResponse.NotImplemented;
		}

		protected override SmtpResponse ProcessCommandEhSC (string name, string arguments)
		{
			return SmtpResponse.NotImplemented;
		}

		protected override SmtpResponse ProcessCommandPush (string name, string arguments)
		{
			return new SmtpResponse(502, "Command not implemented (#5.5.2)", new System.Collections.Generic.List<string> {{
					"Use FeuerwehrCloud.Input.SMTPSever.lua!"
				}
			});
		}

		protected override SmtpResponse ProcessCommandHelp (string name, string arguments)
		{
			return new SmtpResponse (502, "There's no help!", new System.Collections.Generic.List<string> {{
					"You should not use this SMTP-Server for testing purposes!"
				}
			});
		}

		protected override SmtpResponse ProcessCommandDBY(string name, string arguments)
		{
			throw new NotImplementedException();
		}

		protected override SmtpResponse ProcessCommandLR(string name, string arguments)
		{
			throw new NotImplementedException();
		}

        protected override SmtpResponse ProcessCommandVrfy(string name, string arguments)
        {
            var notIdentified = CreateResponseIfNotIdentified();
            if (notIdentified.HasValue) return notIdentified;

            if (String.IsNullOrWhiteSpace(arguments))
            {
				return new SmtpResponse(501, "VRFY Missing parameter. (#5.5.0)");
            }

            var response = _responderFactory.VerifyResponder.Verify(SessionInfo, arguments);
            return response;
        }

        private SmtpResponse CreateResponseIfNotIdentified()
        {
            if (SessionInfo.Identification.Mode == SmtpIdentificationMode.NotIdentified)
            {
                return SmtpResponses.NotIdentified;
            }
            return SmtpResponses.None;
        }

        private SmtpResponse CreateResponseIfHasNotMailFrom()
        {
            if (SessionInfo.MailFrom == null)
            {
				return new SmtpResponse(503, "Use MAIL FROM first. (#5.5.1)");
            }
            return SmtpResponse.None;
        }

        private SmtpResponse CreateResponseIfHasNoRecipients()
        {
            return !SessionInfo.Recipients.Any() 
                ? SmtpResponses.MustHaveRecipientFirst 
                : SmtpResponses.None;
        }

        protected override SmtpResponse ProcessRawLine(string line)
        {
			//            MailServerLogger.Instance.Debug("<<< " + line);

            var response = _responderFactory.RawLineResponder.RawLine(SessionInfo, line);
            return response;
        }

        protected override SmtpResponse ProcessCommandDataEnd()
        {
            MailServerLogger.Instance.Debug("DataEnd received"); 
            
            var response = _responderFactory.DataResponder.DataEnd(SessionInfo);
            if (response.Success)
            {
                InDataMode = false;
            }
            return response;
        }

        protected override SmtpResponse ProcessDataLine(byte[] line)
        {
            var response = _responderFactory.DataResponder.DataLine(SessionInfo, line);
            return response;
        }
    }
}
