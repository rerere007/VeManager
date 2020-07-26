using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeManagerApp
{
    class Constants
    {
        public const short NO_TASK = 0;
        public const short SHOW_IMAGE_TASK = 1;
        public const short CINE_IMAGE_TASK = 2;
        public const short HUE_IMAGE_TASK = 3;
        public const short FACE_IMAGE_TASK = 4;

        private int current_task_flag = 0;
        private object task_flag_key = new object();

        public Constants()
        {
            this.current_task_flag = NO_TASK;
        }

        public void change_task(int current_task)
        {
            lock (task_flag_key)
            {
                this.current_task_flag = current_task;
            }
        }

        public int getCurrentTask()
        {
            lock (task_flag_key)
            {
                return this.current_task_flag;

            }
        }
        public void init()
        {
            lock (task_flag_key)
            {
                this.current_task_flag = NO_TASK;
            }
        }

    }

}
