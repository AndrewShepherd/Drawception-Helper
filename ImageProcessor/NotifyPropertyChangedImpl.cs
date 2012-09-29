using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    // Adding this comment just to see if the cl-lf problem gets fixed.
    public abstract class NotifyPropertyChangedImpl<T, U> : INotifyPropertyChanged where U : INotificationDispatcher, new()
    {
        private readonly U _dispatcher = new U();

        public event PropertyChangedEventHandler PropertyChanged;

        public static string PropertyName<U>(Expression<Func<T, U>> propGetter)
        {
            Expression expression = propGetter.Body;
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpression = expression as MemberExpression;
                return memberExpression.Member.Name;
            }
            return String.Empty;
        }

        protected void Notify<V>(Expression<Func<T, V>> propGetter)
        {
            Action act =  () =>
            {
                if (this.PropertyChanged != null)
                {
                   this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName(propGetter)));
                }
            };
            _dispatcher.Begin(act);
        }
    }
}
