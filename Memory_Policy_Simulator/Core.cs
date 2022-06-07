using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory_Policy_Simulator
{
    class Core
    {
        private string policy;
        private int cursor;
        public int p_frame_size;
        public List<Page> frame_window;
        public List<Page> pageHistory;
        public List<int> frame_age; //LRU를 사용하기 위한 List
        public List<int> count;     //LFU, MFU를 사용하기 위한 List
        public int hit;
        public int fault;
        public int migration;

        public Core(int get_frame_size, string policy)
        {
            this.policy = policy;
            this.cursor = 0;
            this.p_frame_size = get_frame_size;
            switch(policy)
            {
                case "LRU":
                    frame_age = new List<int>();
                    break;
                case "LFU":
                    count = new List<int>();
                    break;
                case "MFU":
                    count = new List<int>();
                    break;
                default:
                    break;
            }
            this.frame_window = new List<Page>();
            this.pageHistory = new List<Page>();
        }
        public Page.STATUS Operate(char data)
        {
            switch (policy)
            {
                case "LRU":
                    return LRUOperate(data);
                case "LFU":
                    return LFUOperate(data);
                case "MFU":
                    return MFUOperate(data);
                default:
                    return FIFOOperate(data);
            }
        }
        public Page.STATUS FIFOOperate(char data)
        {   /*data를 참조 정책 : FIFO*/
            Page newPage;
            if (this.frame_window.Any<Page>(x => x.data == data))
            {   //hit
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data) break;
                }
                newPage.loc = i + 1;

            }
            else
            {   //hit가 아닐 경우
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {   //frame이 꽉 찬 상태
                    newPage.status = Page.STATUS.MIGRATION;
                    this.frame_window.RemoveAt(0);
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else
                {   //frame이 안찬 상태 ( page fault )
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.Add(newPage);

            }
            pageHistory.Add(newPage);

            return newPage.status;
        }
        
        public Page.STATUS LRUOperate(char data)
        {   /*data를 참조 정책 : LRU*/
            Page newPage;
            if (this.frame_window.Any<Page>(x => x.data == data))
            {   //hit
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data) break;
                }
                frame_age[i] = 0;   //참조하였으므로 age 초기화
                newPage.loc = i+1;

            }
            else
            {   //hit가 아닐 경우
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                if (frame_window.Count >= p_frame_size)
                {   //frame이 꽉 찬 상태
                    int maxAge = -1;
                    int maxindex = -1;
                    int temp;
                    newPage.status = Page.STATUS.MIGRATION;
                    for(int i=0; i<frame_window.Count; i++)
                    {
                        temp = frame_age.ElementAt(i);
                        if(temp > maxAge )
                        {
                            maxAge = temp;
                            maxindex = i;
                        }

                    }
                    this.frame_window.RemoveAt(maxindex);
                    frame_age.RemoveAt(maxindex);
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else
                {   //frame이 안찬 상태 ( page fault )
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }
                newPage.loc = cursor;
                frame_age.Add(0);
                frame_window.Add(newPage);
            }
            for (int i = 0; i < frame_age.Count; i++)
            {
                frame_age[i]++;
            }
            pageHistory.Add(newPage);

            return newPage.status;
        }
        
        public Page.STATUS LFUOperate(char data)
        {   /*data를 참조 정책 : LFU*/
            Page newPage;
            if (this.frame_window.Any<Page>(x => x.data == data))
            {   //hit
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data) break;
                }
                count[i]++;
                newPage.loc = i + 1;

            }
            else
            {   //hit가 아닐 경우
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {   //frame이 꽉 찬 상태
                    int leastcount = int.MaxValue;
                    int index = -1;
                    int temp;
                    newPage.status = Page.STATUS.MIGRATION;
                    for(int i=0; i<count.Count; i++)
                    {
                        temp = count.ElementAt(i);
                        if(leastcount > temp)
                        {
                            leastcount = temp;
                            index = i;
                        }
                    }
                    this.frame_window.RemoveAt(index);
                    count.RemoveAt(index);
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else
                {   //frame이 안찬 상태 ( page fault )
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                count.Add(0);
                frame_window.Add(newPage);
            }
            pageHistory.Add(newPage);

            return newPage.status;
        }
        public Page.STATUS MFUOperate(char data)
        {   /*data를 참조 정책 : Optimal*/
            Page newPage;
            if (this.frame_window.Any<Page>(x => x.data == data))
            {   //hit
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                this.hit++;
                int i;

                for (i = 0; i < this.frame_window.Count; i++)
                {
                    if (this.frame_window.ElementAt(i).data == data) break;
                }
                count[i]++;
                newPage.loc = i + 1;

            }
            else
            {   //hit가 아닐 경우
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size)
                {   //frame이 꽉 찬 상태
                    int maxcount = -1;
                    int index = -1;
                    int temp;
                    for (int i = 0; i < count.Count; i++)
                    {
                        temp = count.ElementAt(i);
                        if(temp > maxcount)
                        {
                            maxcount = temp;
                            index = i;
                        }
                    }
                    newPage.status = Page.STATUS.MIGRATION;
                    this.frame_window.RemoveAt(index);
                    this.count.RemoveAt(index);
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else
                {   //frame이 안찬 상태 ( page fault )
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.Add(newPage);
                count.Add(0);
            }
            pageHistory.Add(newPage);
            
            return newPage.status;
        }
        public List<Page> GetPageInfo(Page.STATUS status)
        {
            List<Page> pages = new List<Page>();

            foreach (Page page in pageHistory)
            {
                if (page.status == status)
                {
                    pages.Add(page);
                }
            }

            return pages;
        }

    }


}