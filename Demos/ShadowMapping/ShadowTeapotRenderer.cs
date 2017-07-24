﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpGL;

namespace ShadowMapping
{
    /// <summary>
    /// render a teapot with shadow.
    /// </summary>
    class ShadowTeapotRenderer : ModernNode, IShadowMapping
    {
        private const string inPosition = "inPosition";
        private const string inNormal = "inNormal";
        private const string mvpMatrix = "mvpMatrix";
        private const string model_matrix = "model_matrix";
        private const string view_matrix = "view_matrix";
        private const string projection_matrix = "projection_matrix";
        private const string shadow_matrix = "shadow_matrix";
        private const string depth_texture = "depth_texture";
        private const string light_position = "light_position";
        private const string material_ambient = "material_ambient";
        private const string material_diffuse = "material_diffuse";
        private const string material_specular = "material_specular";
        private const string material_specular_power = "material_specular_power";

        private const string shadowVertexCode =
            @"#version 330

uniform mat4 " + mvpMatrix + @";

layout (location = 0) in vec4 " + inPosition + @";;

void main(void)
{
	gl_Position = mvpMatrix * inPosition;
}
";
        // this fragment shader is not needed.
        //        private const string shadowFragmentCode =
        //            @"#version 330 core
        //
        //layout(location = 0) out float fragmentdepth;
        ////out vec4 out_Color;
        //
        //void main(void) {
        //    fragmentdepth = gl_FragCoord.z;
        //
        //}
        //";

        private const string lightVertexCode =
            @"#version 330

uniform mat4 " + model_matrix + @";
uniform mat4 " + view_matrix + @";
uniform mat4 " + projection_matrix + @";

uniform mat4 " + shadow_matrix + @";

layout (location = 0) in vec4 " + inPosition + @";
layout (location = 1) in vec3 " + inNormal + @";

out VS_FS_INTERFACE
{
	vec4 shadow_coord;
	vec3 world_coord;
	vec3 eye_coord;
	vec3 normal;
} vertex;

void main(void)
{
	vec4 world_pos = model_matrix * inPosition;
	vec4 eye_pos = view_matrix * world_pos;
	vec4 clip_pos = projection_matrix * eye_pos;
	
	vertex.world_coord = world_pos.xyz;
	vertex.eye_coord = eye_pos.xyz;
	vertex.shadow_coord = shadow_matrix * world_pos;
	vertex.normal = mat3(view_matrix * model_matrix) * inNormal;
	
	gl_Position = clip_pos;
}
";
        private const string lightFragmentCode =
            @"#version 330

uniform sampler2DShadow " + depth_texture + @";
uniform vec3 " + light_position + @";

uniform vec3 " + material_ambient + @";
uniform vec3 " + material_diffuse + @";
uniform vec3 " + material_specular + @";
uniform float " + material_specular_power + @";

layout (location = 0) out vec4 color;

in VS_FS_INTERFACE
{
	vec4 shadow_coord;
	vec3 world_coord;
	vec3 eye_coord;
	vec3 normal;
} fragment;

void main(void)
{
    vec3 N = fragment.normal;
    vec3 L = normalize(light_position - fragment.world_coord);
    float LdotN = dot(N, L);
    vec3 R = reflect(-L, N);

    float diffuse = max(LdotN, 0.0);
    float specular = max(pow(dot(normalize(-fragment.eye_coord), R), material_specular_power), 0.0);

    float f = textureProj(depth_texture, fragment.shadow_coord);

    color = vec4(material_ambient + f * (material_diffuse * diffuse + material_specular * specular), 1.0);
    //color = vec4(f,f,0,1) * 0.5 + color * 0.01f;

}
";

        /// <summary>
        /// Render teapot to framebuffer in modern opengl.
        /// </summary>
        /// <returns></returns>
        public static ShadowTeapotRenderer Create()
        {
            RenderUnitBuilder shadowBuilder, lightBuilder;
            {
                var vs = new VertexShader(shadowVertexCode, inPosition);
                var provider = new ShaderArray(vs);
                var map = new AttributeMap();
                map.Add(inPosition, Teapot.strPosition);
                shadowBuilder = new RenderUnitBuilder(provider, map);
            }
            {
                var vs = new VertexShader(lightVertexCode, inPosition, inNormal);
                var fs = new FragmentShader(lightFragmentCode);
                var provider = new ShaderArray(vs, fs);
                var map = new AttributeMap();
                map.Add(inPosition, Teapot.strPosition);
                map.Add(inNormal, Teapot.strNormal);
                lightBuilder = new RenderUnitBuilder(provider, map);
            }
            var model = new Teapot();
            var renderer = new ShadowTeapotRenderer(model, shadowBuilder, lightBuilder);
            renderer.Initialize();

            return renderer;
        }

        private ShadowTeapotRenderer(Teapot model, params RenderUnitBuilder[] builder)
            : base(model, builder)
        {
            this.ModelSize = model.GetModelSize();
            this.Ambient = new vec3(1, 1, 1) * 0.1f;
            this.Diffuse = System.Drawing.Color.SkyBlue.ToVec3();
            this.Specular = new vec3(1, 1, 1) * 0.1f;
            this.SpecularPower = 0.2f;
        }

        #region IRenderable 成员

        public float RotateSpeed { get; set; }

        public vec3 Ambient { get; set; }
        public vec3 Diffuse { get; set; }
        public vec3 Specular { get; set; }
        public float SpecularPower { get; set; }

        public override void RenderBeforeChildren(RenderEventArgs arg)
        {
            if (!this.IsInitialized) { Initialize(); }

            this.RotationAngle += this.RotateSpeed;

            ICamera camera = arg.CameraStack.Peek();
            mat4 projection = camera.GetProjectionMatrix();
            mat4 view = camera.GetViewMatrix();
            mat4 model = this.GetModelMatrix();
            List<LightBase> lights = arg.CurrentLights.Peek();
            LightBase light = lights[0];// now we only use one light for testing.
            mat4 lightBias = glm.scale(mat4.identity(), new vec3(1, 1, 1) * 0.5f);
            lightBias = glm.translate(lightBias, new vec3(1, 1, 1) * 0.5f);
            mat4 lightProjection = light.GetProjectionMatrix();
            mat4 lightView = light.GetViewMatrix();

            var renderUnit = this.RenderUnits[1]; // the only render unit in this renderer.
            ShaderProgram program = renderUnit.Program;
            program.SetUniform(mvpMatrix, projection * view * model);
            program.SetUniform(model_matrix, model);
            program.SetUniform(view_matrix, view);
            program.SetUniform(projection_matrix, projection);
            program.SetUniform(shadow_matrix, lightProjection * lightView);
            program.SetUniform(depth_texture, light.BindingTexture);
            program.SetUniform(light_position, light.Position);
            program.SetUniform(material_ambient, this.Ambient);
            program.SetUniform(material_diffuse, this.Diffuse);
            program.SetUniform(material_specular, this.Specular);
            program.SetUniform(material_specular_power, this.SpecularPower);

            renderUnit.Render();
        }

        public override void RenderAfterChildren(RenderEventArgs arg)
        {
        }

        #endregion


        #region IShadowMapping 成员

        private bool enableShadowMapping = true;

        public bool EnableShadowMapping
        {
            get { return enableShadowMapping; }
            set { enableShadowMapping = value; }
        }

        public void CastShadow(ShdowMappingEventArgs arg)
        {
            if (!this.IsInitialized) { this.Initialize(); }

            this.RotationAngle += this.RotateSpeed;

            LightBase light = arg.CurrentLight;
            mat4 projection = light.GetProjectionMatrix();
            mat4 view = light.GetViewMatrix();
            mat4 model = this.GetModelMatrix();

            var renderUnit = this.RenderUnits[0]; // the only render unit in this renderer.
            ShaderProgram program = renderUnit.Program;
            program.SetUniform(mvpMatrix, projection * view * model);

            renderUnit.Render();
        }

        #endregion
    }
}