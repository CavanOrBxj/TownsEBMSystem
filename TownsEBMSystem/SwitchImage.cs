/********************************************************************
 * *
 * * Copyright (C) 2013-? Corporation All rights reserved.
 * * 作者： BinGoo QQ：315567586 
 * * 请尊重作者劳动成果，请保留以上作者信息，禁止用于商业活动。
 * *
 * * 创建时间：2014-12-30
 * * 说明：
 * *
********************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TownsEBMSystem
{
    public class SwitchImage : Panel
    {
        #region 变量
        private Timer _timer;
        /// <summary>
        /// 图片信息左边距
        /// </summary>
        private int _infoPaddingLeft = 20;
        /// <summary>
        /// 当前图片下标
        /// </summary>
        private int _pageIndex = 0;
        /// <summary>
        /// 上一张图片下标
        /// </summary>
        private int _oldpageIndex = 0;

        private AnimatorDirection _imageDirection = AnimatorDirection.LeftToRight;
        /// <summary>
        /// 图片列表
        /// </summary>
        protected List<ImageModle> ImageList = new List<ImageModle>();
        private ViewButton leftButton;
        private ViewButton rightButton;
        /// <summary>
        /// 切换图片是否启动动画
        /// </summary>
        private bool _isShowAnimation=true;
        /// <summary>
        /// 启动动画时切换速度
        /// </summary>
        private int _animationSpeed = 50;
        #endregion

        #region 属性
        /// <summary>
        /// 是否显示动画
        /// </summary>
        [Description("是否显示动画")]
        public bool IsShowAnimation
        {
            get { return _isShowAnimation; }
            set { _isShowAnimation = value; }
        }

        /// <summary>
        /// 动画显示速度
        /// </summary>
        [Description("动画显示速度")]
        public int AnimationSpeed
        {
            get
            {
                if (_animationSpeed < 1)
                {
                    _animationSpeed = 1;
                }
                return _animationSpeed;
            }
            set
            {

                _animationSpeed = value;
            }
        }

        /// <summary>
        /// 图片信息框高度
        /// </summary>
        [Description("图片信息框高度")]
        public int InfoHeight
        {
            set;
            get;
        }
        /// <summary>
        /// 图片信息文字左边距
        /// </summary>
        [Description("图片信息文字左边距")]
        public int InfoPaddingLeft
        {
            set
            {
                _infoPaddingLeft = value;
            }
            get
            {
                return _infoPaddingLeft;
            }
        }
        /// <summary>
        /// 信息框背景色
        /// </summary>
        [Description("信息框背景色")]
        public int InfoOpacity
        {
            set;
            get;
        }
        #endregion

        #region 构造函数
        public SwitchImage()
        {
            SetStyle(
              ControlStyles.UserPaint |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.ResizeRedraw |
              ControlStyles.DoubleBuffer, true);
            UpdateStyles();
            //强制分配样式重新应用到控件上
            UpdateStyles();
            //CaptionAnimationSpeed = 50;
            IsShowAnimation = true;
            InfoHeight = 40;
            InfoOpacity = 120;
            AnimationSpeed = 30;
            leftButton = new ViewButton();
            leftButton.Text = string.Format("<");
            leftButton.Click +=leftButton_Click;

            rightButton = new ViewButton();
            rightButton.Text = string.Format(">");
            rightButton.Click += rightButton_Click;

            Resize += ImageSlider_Resize;

            Controls.Add(leftButton);
            Controls.Add(rightButton);

        } 
        #endregion

        #region 事件
        /// <summary>
        /// 控件大小改变时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageSlider_Resize(object sender, EventArgs e)
        {
            leftButton.Location = new Point(0, (Height / 2) - (leftButton.Height / 2));
            rightButton.Location = new Point(Width - rightButton.Width, (Height / 2) - (rightButton.Height / 2));
        }

        #region 左右切换按钮事件
        private void leftButton_Click(object sender, EventArgs e)
        {
            //从右向左显示动画
            if (_pageIndex > 0)
            {
                //如果不是最后一张则显示下一张
                _oldpageIndex = _pageIndex;
                --_pageIndex;
                
                _imageDirection = AnimatorDirection.RightToLeft;
            }
            else
            {
                //如果是最后一张显示第一张
                _imageDirection = AnimatorDirection.RightToLeft;
                _oldpageIndex = _pageIndex;
                _pageIndex = ImageList.Count - 1;
            }
            //是否以动画效果显示图片
            if (IsShowAnimation)
            {
                if (_imageDirection == AnimatorDirection.LeftToRight)
                {
                    InfoPaddingLeft = -Width;
                }
                else if (_imageDirection == AnimatorDirection.RightToLeft)
                {
                    InfoPaddingLeft = Width + 40;
                }
                DoubleBuffered = true;

                _timer = new Timer();
                _timer.Interval = AnimationSpeed;
                _timer.Tick += AnimationTick;
                _timer.Start();
            }
            else
            {
                InfoPaddingLeft = 20;
                Invalidate();
            }
        }

        private void rightButton_Click(object sender, EventArgs e)
        {
            //从左往右显示动画
            if (_pageIndex < ImageList.Count - 1)
            {
                //如果不是第一张则显示前一张
                _imageDirection = AnimatorDirection.LeftToRight;
                _oldpageIndex = _pageIndex;
                ++_pageIndex;
            }
            else
            {
                //如果是第一张显示最后一张
                _imageDirection = AnimatorDirection.LeftToRight;
                _oldpageIndex = _pageIndex;
                _pageIndex = 0;
            }
            //是否以动画效果显示图片
            if (IsShowAnimation)
            {
                if (_imageDirection == AnimatorDirection.LeftToRight)
                {
                    InfoPaddingLeft = -Width;
                }
                else if (_imageDirection == AnimatorDirection.RightToLeft)
                {
                    InfoPaddingLeft = Width + 40;
                }

                DoubleBuffered = true;

                _timer = new Timer();
                _timer.Interval = AnimationSpeed;
                _timer.Tick += AnimationTick;
                _timer.Start();
            }
            else
            {
                _oldpageIndex = _pageIndex;
                InfoPaddingLeft = 20;
                Invalidate();
            }
        }
        #endregion

        /// <summary>
        /// 动画定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimationTick(object sender, EventArgs e)
        {
            if (_imageDirection == AnimatorDirection.LeftToRight)
            {
                if (InfoPaddingLeft <= 20)
                {
                    InfoPaddingLeft += Width / 10;
                    Invalidate();
                }
                else
                {
                    InfoPaddingLeft = 20;
                    DoubleBuffered = false;
                    _timer.Dispose();
                }
            }
            else if (_imageDirection == AnimatorDirection.RightToLeft)
            {
                if (InfoPaddingLeft >= 20)
                {
                    InfoPaddingLeft -= Width / 10;
                    Invalidate();
                }
                else
                {
                    InfoPaddingLeft = 20;
                    DoubleBuffered = false;
                    _timer.Dispose();
                }
            }



        } 
        #endregion

        #region 添加切换背景图片方法
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="path">image路径</param>
        public void AddImageItems(string path)
        {
            Image img = Image.FromFile(path);
            AddImageItems(img, "", "", BackColor);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="path">image路径</param>
        /// <param name="info">图片信息</param>
        public void AddImageItems(string path, string info)
        {
            Image img = Image.FromFile(path);
            AddImageItems(img, "", info, BackColor);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="path">image路径</param>
        /// <param name="info">图片信息</param>
        /// <param name="infoBackColor">图片信息背景框颜色</param>
        public void AddImageItems(string path, string info, Color infoBackColor)
        {
            Image img = Image.FromFile(path);
            AddImageItems(img, "", info, infoBackColor);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="img">Image图片</param>
        public void AddImageItems(Image img)
        {
            AddImageItems(img, "", "", Color.White);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="img">Image图片</param>
        /// <param name="info">图片信息</param>
        public void AddImageItems(Image img, string info)
        {
            AddImageItems(img, "", info, Color.White);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="img">Image图片</param>
        /// <param name="info">图片信息</param>
        /// <param name="infoBackColor">图片信息背景框颜色</param>
        public void AddImageItems(Image img, string info, Color infoBackColor)
        {
            AddImageItems(img, "", info, infoBackColor);
        }
        /// <summary>
        /// 添加image（其他信息系统默认为空）
        /// </summary>
        /// <param name="img">Image图片</param>
        /// <param name="title">图片标题</param>
        /// <param name="info">图片信息</param>
        /// <param name="infoBackColor">图片信息背景框颜色</param>
        public void AddImageItems(Image img, string title, string info, Color infoBackColor)
        {
            ImageList.Add(new ImageModle(img, title, info, infoBackColor));
        } 
        #endregion
        
        /// <summary>
        /// 重写OnPaint事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Color captionBgColor = Color.FromArgb(InfoOpacity, ImageList[_pageIndex].InformationBackColor);
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality; //高质量
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量
                //绘制上一张图片作为背景图片
                e.Graphics.DrawImage(ImageList[_oldpageIndex].SourcesImage, new Rectangle(0, 0, Width, Height));
                //确定最后一针动画的位置（由于图片大小不确定）
                if (_imageDirection == AnimatorDirection.RightToLeft)
                {
                    InfoPaddingLeft = InfoPaddingLeft <= 20 ? 20 : InfoPaddingLeft;
                }
                else if (_imageDirection == AnimatorDirection.LeftToRight)
                {
                    InfoPaddingLeft = InfoPaddingLeft >= 20 ? 20 : InfoPaddingLeft;
                }
                //绘制下一张正在切换的图片
                e.Graphics.DrawImage(ImageList[_pageIndex].SourcesImage,
                        new Rectangle(InfoPaddingLeft - 20, 0, Width, Height));
                //绘制图片信息框背景
                e.Graphics.FillRectangle(new SolidBrush(captionBgColor),
                    new Rectangle(0, Height - InfoHeight, Width, Height));
               
                string info = ImageList[_pageIndex].Infomation;
                SizeF fontSize = e.Graphics.MeasureString(info, Font);
                //绘制文字
                e.Graphics.DrawString(info, Font, new SolidBrush(ForeColor), InfoPaddingLeft,
                    Height - (int) (InfoHeight - (fontSize.Height/2)));
            }
            catch
            {
            }
            base.OnPaint(e);
        }

        /// <summary>
        /// 自定义按钮类
        /// </summary>
        public class ViewButton : Button
        {
            public ViewButton()
            {
                BackColor = Color.DarkGray;
                Height = 50;
                Width = 50;
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                Graphics g = pevent.Graphics;
                Rectangle area = new Rectangle(0, 0, Width, Height);

                g.FillRectangle(new SolidBrush(BackColor), area);
                SizeF fontSize = g.MeasureString(Text, Font);

                g.DrawString(Text, Font, new SolidBrush(ForeColor), (Width - fontSize.Width)/2,
                    (Height - fontSize.Height)/2);
            }
        }
        /// <summary>
        /// 图片信息类
        /// </summary>
        public class ImageModle 
        {
            /// <summary>
            /// 标题
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// 图片信息
            /// </summary>
            public string Infomation { get; set; }
            /// <summary>
            /// 原始图片
            /// </summary>
            public Image SourcesImage { get; set; }
            /// <summary>
            /// 图片信息栏背景色
            /// </summary>
            public Color InformationBackColor { get; set; }

            public ImageModle(Image img,string title,string info,Color infoBackColor)
            {
                SourcesImage = img;
                Title = title;
                Infomation = info;
                InformationBackColor = infoBackColor;
            }
        }
        /// <summary>
        /// 动画切换方向枚举
        /// </summary>
        public enum AnimatorDirection
        {
            /// <summary>
            /// 从左至右动画
            /// </summary>
            LeftToRight,
            /// <summary>
            /// 从右到左动画
            /// </summary>
            RightToLeft,
        }
    }
}