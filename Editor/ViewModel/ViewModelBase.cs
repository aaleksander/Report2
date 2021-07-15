using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Input;
using System.Linq;
using Commands;
//using IOC;
using Model;

namespace ViewModel
{
	/// <summary>
	/// поле, обозначенное этим аттрибутом, будет обнуляться при allPropertyChanged
	/// </summary>
	public class ExternalAttribute: Attribute
	{
		private object _val;
		
		public ExternalAttribute(object a)
		{
			_val = a;
		}
		
		public object GetEmptyValue()
		{
			return _val;
		}
	}

	/// <summary>
	/// Description of ViewModelBase.
	/// </summary>
	public class ViewModelBase<T>:INotifyPropertyChanged
		where T: ModelBase
	{
		protected static string dateFormat = "d";

		public int HashCode{
			get{
				return this.GetHashCode();
			}
		}

		/// <summary>
		/// Надо ли делать апдейт при изменении свойств
		/// </summary>
		private bool _realTimeUpdate = false;

        protected T _data;

        public T Data
        {
            set
            {
                _data = value;
                OnAllPropertyChanged();
            }
            get
            {
                return _data;
            }
        }

        public void SetData(T aD)
        {//когда не нужно вызывать OnAllPro...
        	_data = aD;
        }
        
        
        public ViewModelBase(T a)
        {
            _data = a;
            _changed = false;
            
        }

		public bool RealTimeUpdate{
			set{
				_realTimeUpdate = value;
			}
			get{
				return _realTimeUpdate;
			}
		}

		public ViewModelBase()
		{
            _data = default(T);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void onPropertyChanged(params string[] aProp){
			foreach(var a in aProp )
			{
				if( a.Trim() != "" )
					_onPropertyChanged(new PropertyChangedEventArgs(a));
			}
		}

		protected void _onPropertyChanged(PropertyChangedEventArgs e)
		{
			if( PropertyChanged != null )
				PropertyChanged(this, e);
		}

		public void OnAllPropertyChanged()
		{//Получить все свойcтва и послать сигнал об их обновлении
			_onPropertyChanged(new PropertyChangedEventArgs(null));

			//пройтись по свойствам и, если есть аттрибут External - обналлить их			
			PropertyInfo[] props = this.GetType().GetProperties();

	        foreach (var prop in props)
	        {
	        	object val;
	        	var attrs = prop.GetCustomAttributes(typeof(ExternalAttribute), false);
	        	if( attrs.Length > 0 )
	        	{
	        		val = ((ExternalAttribute)attrs[0]).GetEmptyValue();
	        		prop.SetValue(this, Convert.ChangeType(val, prop.PropertyType), null);
	        		onPropertyChanged(prop.Name);
	        	}
	        }
		}

        //некое универсальное свойство, что мол было изменено
		public bool Changed{
			set{
				_changed = value;
				//OnAllPropertyChanged();
				_onPropertyChanged(new PropertyChangedEventArgs("Changed"));
			}
			get{
				return _changed;
			}			
		}
		private bool _changed = false;

        /// <summary>
        /// Автоматический порядковый номер в списке
        /// </summary>
        public int AutoPP
        {
            set
            {
                _autoPP = value;
                onPropertyChanged("PP");
            }
            get
            {
                return _autoPP;
            }
        }

        private int _autoPP;

		/// <summary>
		/// обновить, если нужно
		/// </summary>
		protected virtual void UpdateIfNeed()
		{
			if ( RealTimeUpdate ) 
			{
//				if( _storage == null )
//					throw new Exception("Storage не определено");
//
//				_storage.Update(_data);
			}
		}

		//новая уставка для группы
		public delegate void SendEventHandler(object sender, object e);
		public event SendEventHandler Event;

		public void SendEvent(object aMsg)
        {
			if( Event != null )
         		Event(this, aMsg);
        }

		/// <summary>
		/// просто пробрасываем сообщение выше
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void OnEvent(object sender, object args)
		{ 
			if( args is Collection<object>)
			{
				Collection<object> ar = (Collection<object>) args;
				ar.Add(sender);
				SendEvent(ar);
			}
			else
			{
				Collection<object> ar = new Collection<object>();
				ar.Add((object)sender);
				SendEvent(ar);
			}			
		}

		public bool IsSelected{
			set{
				_select = value;
				onPropertyChanged("IsSelected");
			}
			get{
				return _select;
			}
		}
		private bool _select = false;

		public string LastError{
			set{
				_lastError = value;
				onPropertyChanged("LastError");
			}
			get{
				return _lastError;
			}
		}
		private string _lastError = "";

		public override int GetHashCode()
		{
			if( _data == null )
				return -1;
			return _data.GetHashCode();
		}

		#region IOC
//		public IStorage Storage{
//			get{
//				return _storage;
//			}
//			set{
//				_storage = value;
//			}
//		}
//		private IStorage _storage = null;
		#endregion IOC
	}
}
