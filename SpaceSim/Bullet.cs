﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;


namespace SpaceSim
{
	class Bullet : Sphere
	{
		public Vector3 Position;
		public Vector3 Velocity;

		public Bullet(Vector3 position, Vector3 velocity)
		  : base(Matrix.CreateScale(0.005f) * Matrix.CreateTranslation(position), Color.White, 20, 1f, 0f)
		{
			this.Position = position;
			this.Velocity = velocity;
		}

		public void Update(float elapsed)
		{
			this.Position += this.Velocity * elapsed;
			this.transform = Matrix.CreateScale(0.005f) * Matrix.CreateTranslation(this.Position);
		}
	}
}
