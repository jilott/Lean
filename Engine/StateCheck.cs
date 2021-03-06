﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System.Threading;
using QuantConnect.Logging;

namespace QuantConnect.Lean.Engine
{

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Algorithm status monitor reads the central command directive for this algorithm/backtest. When it detects
    /// the backtest has been deleted or cancelled the backtest is aborted.
    /// </summary>
    public static class StateCheck
    {
        /// DB Ping Class
        public static class Ping
        {
            // set to true to break while loop in Run()
            private static bool _exitTriggered;

            /// DB Ping Run Method:
            public static void Run()
            {
                //Don't run at all if local.
                if (Engine.IsLocal) return;

                while (!_exitTriggered)
                {
                    if (AlgorithmManager.AlgorithmId != "" && AlgorithmManager.QuitState == false)
                    {
                        try
                        {
                            //Get the state from the central server:
                            var state = Engine.Api.GetAlgorithmStatus(AlgorithmManager.AlgorithmId);
                            AlgorithmManager.SetStatus(state.Status);
                            //Set which chart the user is look at, so we can reduce excess messaging (e.g. trading 100 symbols, only send 1).
                            Engine.ResultHandler.SetChartSubscription(state.ChartSubscription);
                            Log.Debug("StateCheck.Ping.Run(): Algorithm Status: " + state.Status + " Subscription: " + state.ChartSubscription);
                        }
                        catch
                        {
                            Log.Debug("StateCheck.Run(): Error in state check.");
                        }
                    }
                    else
                    {
                        Log.Debug("StateCheck.Ping.Run(): Opted to not ping: " + AlgorithmManager.AlgorithmId + " " + AlgorithmManager.QuitState);
                    }
                    Thread.Sleep(1000);
                }
            }

            /// <summary>
            /// Send an exit signal to the thread
            /// </summary>
            public static void Exit()
            {
                _exitTriggered = true;
            }
        }
    }
}
