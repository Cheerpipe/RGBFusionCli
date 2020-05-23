using System;
using System.Collections.Generic;
using System.Threading;

namespace RGBFusionCli
{
    public class Transaction
    {
        private bool _transactionStarted = false;
        public bool TransactioStarted { get => _transactionStarted; set => _transactionStarted = value; }
        private Dictionary<int, LedCommand> _transactionLedCmmands;
        private RgbFusion _controller;
        private Timer _transactionMaxAliveTimer;
        private const int DEFAULT_TRANSACTION_TIMEOUT = 5000;

        public void TransactionStart()
        {
            TransactionInitialize();
        }

        public void TransactionStart(int transactionMaxAliveTime)
        {
            TransactionStart();
            if (transactionMaxAliveTime > 0)
            {
                _transactionMaxAliveTimer.Change(transactionMaxAliveTime, transactionMaxAliveTime);
            }
            else
            {
                _transactionMaxAliveTimer.Change(DEFAULT_TRANSACTION_TIMEOUT, DEFAULT_TRANSACTION_TIMEOUT);
            }
        }

        private void TransactionInitialize()
        {
            _transactionLedCmmands = new Dictionary<int, LedCommand>();
            TransactioStarted = true;
        }

        public Transaction(RgbFusion controller)
        {
            _controller = controller;
            _transactionMaxAliveTimer = new Timer(new TimerCallback(_transactionMaxAliveTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void _transactionMaxAliveTimer_Tick(object state)
        {
            _transactionMaxAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (!_transactionStarted) return;
            TransactionCancel();
        }


        public void TransactionSetZoned(List<LedCommand> ledCommands)
        {
            if (!TransactioStarted)
            {
                throw new Exception("Transaction not started");
            }

            foreach (LedCommand ledCommand in ledCommands)
            {
                if (ledCommand.AreaId == -1) // ignore all zones wildward
                    continue;
                _transactionLedCmmands[ledCommand.AreaId] = ledCommand;
            }
        }

        public void TransactionCommit()
        {
            _transactionMaxAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (!TransactioStarted)
            {
                throw new Exception("Transaction not started");
            }

            _controller.ChangeColorForAreas(new List<LedCommand>(_transactionLedCmmands.Values));
            _transactionLedCmmands = null;
            //_transactionMaxAliveTimer.Stop();
            TransactioStarted = false;

        }

        public void TransactionCancel()
        {
            _transactionMaxAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _transactionLedCmmands = null;
            TransactioStarted = false;

        }
    }
}
