﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GXPEngine
{
    internal class Bullet : Sprite
    {

        private string filename;
        private bool addCollider;
        private float bulletSpeed = 2;
        private float trajectory;
        public static Sound bulletHit = new Sound("Gun Hit.wav");
        
        

        public Bullet(string filename,float startX, float startY, float rotation, bool keepInCache = false, bool addCollider = true) : base(filename, keepInCache, addCollider)
        {
            this.filename = filename;
            this.addCollider = addCollider;
            x = startX - (width* 0.2f)/2;
            y = startY;
            trajectory = rotation;
            Console.WriteLine("bullet created");
            scale = 0.2f;
        }

        public void Update()
        {
            if (trajectory == 0)
            {
                y -= bulletSpeed;
            }
            else
            {
                y -= bulletSpeed / (1 + Math.Abs(trajectory));
                x += 2*trajectory;
            }

            
        }

        void OnCollision(GameObject other)
        {
            if (!other.GetType().Equals(typeof(Player)))
            {
                bulletHit.Play();
                flagged = true;
                other.flagged = true;
                
            }
        }
    }
}
