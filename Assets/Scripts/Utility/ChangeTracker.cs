using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utility
{
    public struct ChangeTracker<T>
    {

        public bool state;
        private Func<T> currentValueFunc; // Store a delegate to retrieve the current value  
        public T current;

        public ChangeTracker(Func<T> getCurrentValue)
        {
            state = false;
            currentValueFunc = getCurrentValue;
            current = currentValueFunc();
        }
        public ChangeTracker(T CurrentValue)
        {
            state = false;
            currentValueFunc = null;
            current = CurrentValue;
        }
        public bool Update()
        {
            T currentValue = currentValueFunc();

            if (!current.Equals(currentValue))
            {
                current = currentValue;
                state = !state;
                return true;
            }

            return false;
        }
        public bool Update(T _current)
        {

            if (!current.Equals(_current))
            {
                current = _current;
                state = !state;
                return true;
            }

            return false;
        }
    }

}
