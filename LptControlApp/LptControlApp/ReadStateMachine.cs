using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LptControlApp
{
  
        public enum ReadState
        {
            Idle,
            SendSOF,
            WaitForData,
            Complete,
            Error
        }

        public class ReadStateMachine
        {
            private readonly LptManager lpt;
            private readonly Action<string> log;

            public ReadState State { get; private set; } = ReadState.Idle;

            public ReadStateMachine(LptManager manager, Action<string> logger)
            {
                lpt = manager;
                log = logger;
            }

            public void Start()
            {
                State = ReadState.SendSOF;
                log("READ SM: Starting READ transaction");
            }

            public void Step()
            {
                switch (State)
                {
                    case ReadState.SendSOF:
                        lpt.Send(Commands.SOF);
                        log("READ SM: Sent SOF");
                        State = ReadState.WaitForData;
                        break;

                    case ReadState.WaitForData:
                        var data = lpt.ReadResponse(4);
                        if (data == null)
                        {
                            log("READ SM: No data → ERROR");
                            State = ReadState.Error;
                        }
                        else
                        {
                            log("READ SM: Received DATA: " + BitConverter.ToString(data));
                            State = ReadState.Complete;
                        }
                        break;

                    case ReadState.Complete:
                        log("READ SM: READ transaction complete");
                        State = ReadState.Idle;
                        break;

                    case ReadState.Error:
                        log("READ SM: ERROR");
                        State = ReadState.Idle;
                        break;
                }
            }
        }

    }
 
