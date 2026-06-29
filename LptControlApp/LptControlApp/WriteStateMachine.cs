using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LptControlApp
{
    public enum WriteState
    {
        Idle,
        SendSOF,
        SendCMD,
        SendDATA,
        SendEOF,
        Complete,
        Error
    }

    public class WriteStateMachine
    {
        private readonly LptManager lpt;
        private readonly Action<string> log;

        public WriteState State { get; private set; } = WriteState.Idle;

        private byte[] payload;

        public WriteStateMachine(LptManager manager, Action<string> logger)
        {
            lpt = manager;
            log = logger;
        }

        public void Start(byte[] data)
        {
            payload = data;
            State = WriteState.SendSOF;
            log("WRITE SM: Starting WRITE transaction");
        }

        public void Step()
        {
            switch (State)
            {
                case WriteState.SendSOF:
                    lpt.Send(Commands.SOF);
                    log("WRITE SM: Sent SOF");
                    State = WriteState.SendCMD;
                    break;

                case WriteState.SendCMD:
                    lpt.Send(Commands.CMD);
                    log("WRITE SM: Sent CMD");
                    State = WriteState.SendDATA;
                    break;

                case WriteState.SendDATA:
                    lpt.Send(payload);
                    log("WRITE SM: Sent DATA: " + BitConverter.ToString(payload));
                    State = WriteState.SendEOF;
                    break;

                case WriteState.SendEOF:
                    lpt.Send(Commands.EOF);
                    log("WRITE SM: Sent EOF");
                    State = WriteState.Complete;
                    break;

                case WriteState.Complete:
                    log("WRITE SM: WRITE transaction complete");
                    State = WriteState.Idle;
                    break;

                case WriteState.Error:
                    log("WRITE SM: ERROR");
                    State = WriteState.Idle;
                    break;
            }
        }
    }

}
