using HostChatDemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HostChatDemo.ViewModel
{
    public class LoginVM : BaseVM
    {
        public Action OnConfirm;

        private string userName;

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged(() => UserName);
            }
        }

        private ICommand enterCmd;

        public ICommand EnterCmd
        {
            get
            {
                return enterCmd = enterCmd ?? new RelayCommand(EnterExecute);
            }
        }

        private void EnterExecute(object obj)
        {
            GlobalValue.UserName = UserName;
            OnConfirm?.Invoke();
        }
    }
}
