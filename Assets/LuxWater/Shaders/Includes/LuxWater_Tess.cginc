//  ------------------------------------------------------------------
//  Tessellation

//  Tess additional Inputs
    half    _LuxWater_EdgeLength;
    //float   _TessMaxDisp;
    

#ifdef UNITY_CAN_COMPILE_TESSELLATION

    #include "Tessellation.cginc"
    
    struct TessVertex {
        float4 vertex : INTERNALTESSPOS;
        float4 color : COLOR;
    //  Projectors
        //float3 normal : NORMAL;

        #if !defined(ISWATERVOLUME)
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float4 texcoord : TEXCOORD0;
        #endif
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    struct OutputPatchConstant {
        float edge[3]         : SV_TessFactor;
        float inside          : SV_InsideTessFactor;
    };
    
    TessVertex tessvert (appdata_water v) {
        
        UNITY_SETUP_INSTANCE_ID(v);
        TessVertex o = (TessVertex)0;
        UNITY_TRANSFER_INSTANCE_ID(v,o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    //  We transfer the position to WS here to reduce cracks
        o.vertex        = mul(unity_ObjectToWorld, v.vertex); //v.vertex;
        o.color         = v.color;
    //  Projectors
        //o.normal    = v.normal;

        #if !defined(ISWATERVOLUME)
            o.normal    = v.normal;
            o.tangent   = v.tangent;
            o.texcoord  = v.texcoord;
        #endif
        
        return o;
    }

    float4 LuxEdgeLengthBasedTess (float4 v0, float4 v1, float4 v2, float edgeLength)
    {
        
    //  Input is in WS already!
        float3 pos0 = v0.xyz;
        float3 pos1 = v1.xyz;
        float3 pos2 = v2.xyz;
        float4 tess;
        tess.x = UnityCalcEdgeTessFactor (pos1, pos2, edgeLength);
        tess.y = UnityCalcEdgeTessFactor (pos2, pos0, edgeLength);
        tess.z = UnityCalcEdgeTessFactor (pos0, pos1, edgeLength);
        tess.w = (tess.x + tess.y + tess.z) / 3.0f;
        return tess;
    }


    float4 Tessellation(TessVertex v, TessVertex v1, TessVertex v2) {
        if (v.color.r + v1.color.r + v2.color.r == 0) {
            return float4(1,1,1,1);
        }
        return LuxEdgeLengthBasedTess(v.vertex, v1.vertex, v2.vertex, _LuxWater_EdgeLength);
        // 4.3ms ocean on GTX 970M
        // return UnityEdgeLengthBasedTess(v.vertex, v1.vertex, v2.vertex, _LuxWater_EdgeLength);
        // 4.0ms ocean on GTX 970M
        // return UnityEdgeLengthBasedTessCull(v.vertex, v1.vertex, v2.vertex, _LuxWater_EdgeLength, _TessMaxDisp);
    }

    OutputPatchConstant hullconst (InputPatch<TessVertex,3> v) {
        OutputPatchConstant o;
        float4 ts = Tessellation( v[0], v[1], v[2] );
        ts = clamp(ts, float4(1,1,1,1), float4(31,31,31,31));
        o.edge[0] = ts.x;
        o.edge[1] = ts.y;
        o.edge[2] = ts.z;
        o.inside = ts.w;
        return o;
    }

//  Tessellation hull shader
    [UNITY_domain("tri")]
    [UNITY_partitioning("fractional_odd")]
    [UNITY_outputtopology("triangle_cw")]
    [UNITY_patchconstantfunc("hullconst")]
    [UNITY_outputcontrolpoints(3)]
    TessVertex hs_surf (InputPatch<TessVertex,3> v, uint id : SV_OutputControlPointID) {
        return v[id];
    }

//  tessellation domain shader
    [UNITY_domain("tri")]
    v2f ds_surf (OutputPatchConstant tessFactors, const OutputPatch<TessVertex,3> vi, float3 bary : SV_DomainLocation) {
        
        appdata_water v = (appdata_water)0;
        UNITY_TRANSFER_INSTANCE_ID(vi[0], v); // important

        v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
        v.color = vi[0].color*bary.x + vi[1].color*bary.y + vi[2].color*bary.z;

        // Projectors
        // v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;

        #if !defined(ISWATERVOLUME)
            v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
            v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
            //#ifdef UNITY_PASS_FORWARDADD
            //v.texcoord1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
            //#endif
            v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
        #endif
        // this works - but the macro below does not?
        // #if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
        //    v.instanceID = vi[0].instanceID;
        // #endif
    //  Now call the regular vertex function
        v2f o = vert(v);
        UNITY_TRANSFER_INSTANCE_ID(vi[0], o);
        return o;
    }

#endif