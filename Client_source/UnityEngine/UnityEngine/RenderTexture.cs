using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class RenderTexture : Texture
	{
		public override int width
		{
			get
			{
				return Internal_GetWidth(this);
			}
			set
			{
				Internal_SetWidth(this, value);
			}
		}

		public override int height
		{
			get
			{
				return Internal_GetHeight(this);
			}
			set
			{
				Internal_SetHeight(this, value);
			}
		}

		public int depth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isPowerOfTwo
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public RenderTextureFormat format
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool useMipMap
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isCubemap
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static RenderTexture active
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("Use SystemInfo.supportsRenderTextures instead.")]
		public static bool enabled
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public RenderTexture(int width, int height, int depth, RenderTextureFormat format)
		{
			Internal_CreateRenderTexture();
			this.width = width;
			this.height = height;
			this.depth = depth;
			this.format = format;
		}

		public RenderTexture(int width, int height, int depth)
		{
			Internal_CreateRenderTexture();
			this.width = width;
			this.height = height;
			this.depth = depth;
			format = RenderTextureFormat.Default;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_CreateRenderTexture();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern RenderTexture GetTemporary(int width, int height, int depthBuffer, RenderTextureFormat format);

		public static RenderTexture GetTemporary(int width, int height, int depthBuffer)
		{
			RenderTextureFormat format = RenderTextureFormat.Default;
			return GetTemporary(width, height, depthBuffer, format);
		}

		public static RenderTexture GetTemporary(int width, int height)
		{
			RenderTextureFormat format = RenderTextureFormat.Default;
			int depthBuffer = 0;
			return GetTemporary(width, height, depthBuffer, format);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void ReleaseTemporary(RenderTexture temp);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int Internal_GetWidth(RenderTexture mono);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetWidth(RenderTexture mono, int width);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int Internal_GetHeight(RenderTexture mono);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetHeight(RenderTexture mono, int width);

		public bool Create()
		{
			return INTERNAL_CALL_Create(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern bool INTERNAL_CALL_Create(RenderTexture self);

		public void Release()
		{
			INTERNAL_CALL_Release(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Release(RenderTexture self);

		public bool IsCreated()
		{
			return INTERNAL_CALL_IsCreated(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern bool INTERNAL_CALL_IsCreated(RenderTexture self);

		public void DiscardContents()
		{
			INTERNAL_CALL_DiscardContents(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_DiscardContents(RenderTexture self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetGlobalShaderProperty(string propertyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_GetTexelOffset(RenderTexture tex, out Vector2 output);

		public Vector2 GetTexelOffset()
		{
			Internal_GetTexelOffset(this, out var output);
			return output;
		}

		[Obsolete("RenderTexture.SetBorderColor was removed", true)]
		public void SetBorderColor(Color color)
		{
		}
	}
}
