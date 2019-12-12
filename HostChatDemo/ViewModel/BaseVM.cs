using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HostChatDemo.ViewModel
{
    public class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 属性值变化时发生
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 属性值变化时发生
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = (propertyExpression.Body as MemberExpression).Member.Name;
            this.OnPropertyChanged(propertyName);
        }

        public ICommand onLoadedCmd;

        public ICommand OnLoadedCmd
        {
            get
            {
                return onLoadedCmd = onLoadedCmd ?? new RelayCommand(OnLoadedExecute);
            }
        }

        protected virtual void OnLoadedExecute(object obj) { }

        public ICommand onUnloadedCmd;

        public ICommand OnUnloadedCmd
        {
            get
            {
                return onUnloadedCmd = onUnloadedCmd ?? new RelayCommand(OnUnloadedExecute);
            }
        }

        protected virtual void OnUnloadedExecute(object obj) { }
    }
}
