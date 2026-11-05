# Corridor audit sweep: per-track minimum drivable width vs route corridor.
# Method: turn-15 point-in-triangle coverage, mirrored to DriveSurfaceLoader.cs
# (file x,y,z -> game x,y,-z; f lines take first 3 indices only).
# Per segment: candidates = triangles with centroid within 30 m (XZ) of segment
# midpoint; roadY = median plane-Y of triangles containing the midpoint; walk
# outward in 0.25 m steps to +/-22 m; extent = last covered step before first
# uncovered (first-gap); step continuity |y-yRef|<=2.0 m, |y-roadY|<=8.0 m.
$ErrorActionPreference = 'Stop'
$src = @'
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class CorridorSweep {
    static double[] Tax,Tay,Taz,Tbx,Tby,Tbz,Tcx,Tcy,Tcz,TCX,TCZ;
    static double[] RX,RZ;

    static bool Hit(int t, double px, double pz, out double y) {
        double ax=Tax[t], az=Taz[t], bx=Tbx[t], bz=Tbz[t], cx=Tcx[t], cz=Tcz[t];
        double v0x=bx-ax, v0z=bz-az;
        double v1x=cx-ax, v1z=cz-az;
        double v2x=px-ax, v2z=pz-az;
        double d00=v0x*v0x+v0z*v0z;
        double d01=v0x*v1x+v0z*v1z;
        double d11=v1x*v1x+v1z*v1z;
        double d20=v2x*v0x+v2z*v0z;
        double d21=v2x*v1x+v2z*v1z;
        double den=d00*d11-d01*d01;
        if (den>-1e-12 && den<1e-12) { y=0; return false; }
        double v=(d11*d20-d01*d21)/den;
        double w=(d00*d21-d01*d20)/den;
        if (v<-1e-9 || w<-1e-9 || v+w>1.0+1e-9) { y=0; return false; }
        y=(1.0-v-w)*Tay[t]+v*Tby[t]+w*Tcy[t];
        return true;
    }

    static double Walk(double mx, double mz, double dx, double dz, double roadY, List<int> cand) {
        double yRef=roadY; double last=0.0;
        for (double d=0.25; d<=22.0001; d+=0.25) {
            double px=mx+dx*d, pz=mz+dz*d;
            bool hit=false;
            for (int k=0;k<cand.Count;k++) {
                double y;
                if (Hit(cand[k],px,pz,out y)) {
                    if (Math.Abs(y-yRef)<=2.0 && Math.Abs(y-roadY)<=8.0) { hit=true; yRef=y; break; }
                }
            }
            if (!hit) break;
            last=d;
        }
        return last;
    }

    public static string Run(string tracksFile, string routesPath, string resDir, string outCsv, string outSegCsv) {
        var ci=CultureInfo.InvariantCulture;
        var sb=new StringBuilder();
        var sbSeg=new StringBuilder();
        sb.AppendLine("track,name,map_layout,nRoute,nVerts,nTris,routeLenM,nSeg,deadSeg,minHalf,argSeg,argX,argZ,p10Half,medianHalf,nBelow600,nBelow820,nSat220");
        sbSeg.AppendLine("track,seg,midX,midZ,left,right,perSeg");
        string src=File.ReadAllText(routesPath,Encoding.UTF8);
        int anchor=src.IndexOf("Points={");
        if (anchor<0) return "ERROR no Points anchor";
        string[] chunks=src.Substring(anchor).Split(new string[]{"new Vec[]{"},StringSplitOptions.None);
        if (chunks.Length!=61) return "ERROR chunk count "+chunks.Length;
        var rx=new Regex("new Vec\\((-?[0-9.]+)f,(-?[0-9.]+)f\\)");
        string[] tracks=File.ReadAllLines(tracksFile);
        var cand=new List<int>(8192);
        var ys=new List<double>(64);
        foreach (string line in tracks) {
            if (line.Trim().Length==0) continue;
            string[] p=line.Split('|');
            int idx=int.Parse(p[0]);
            string map=p[2], layout=p[3], name=p[4].Replace(",",";");
            string objPath=Path.Combine(resDir,map+"_"+layout+".txt");
            MatchCollection ms=rx.Matches(chunks[idx+1]);
            int n=ms.Count;
            if (n<4) { sb.AppendLine(idx+","+name+","+map+"_"+layout+",,,,,ERROR_FEW_POINTS,,,,,,,," ); continue; }
            RX=new double[n]; RZ=new double[n];
            for (int i=0;i<n;i++){ RX[i]=double.Parse(ms[i].Groups[1].Value,ci); RZ[i]=double.Parse(ms[i].Groups[2].Value,ci); }
            var vxL=new List<double>(4096); var vyL=new List<double>(4096); var vzL=new List<double>(4096);
            var triL=new List<int>(65536);
            foreach (string l in File.ReadLines(objPath)) {
                if (l.Length<2) continue;
                if (l[0]=='v'&&l[1]==' ') {
                    string[] vp=l.Substring(2).Trim().Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
                    vxL.Add(double.Parse(vp[0],ci));
                    vyL.Add(double.Parse(vp[1],ci));
                    vzL.Add(-double.Parse(vp[2],ci));
                } else if (l[0]=='f'&&l[1]==' ') {
                    string[] fp=l.Substring(2).Trim().Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
                    triL.Add(int.Parse(fp[0])-1);
                    triL.Add(int.Parse(fp[1])-1);
                    triL.Add(int.Parse(fp[2])-1);
                }
            }
            int nV=vxL.Count, nT=triL.Count/3;
            Tax=new double[nT]; Tay=new double[nT]; Taz=new double[nT];
            Tbx=new double[nT]; Tby=new double[nT]; Tbz=new double[nT];
            Tcx=new double[nT]; Tcy=new double[nT]; Tcz=new double[nT];
            TCX=new double[nT]; TCZ=new double[nT];
            for (int t=0;t<nT;t++) {
                int a=triL[t*3], b=triL[t*3+1], c=triL[t*3+2];
                Tax[t]=vxL[a]; Tay[t]=vyL[a]; Taz[t]=vzL[a];
                Tbx[t]=vxL[b]; Tby[t]=vyL[b]; Tbz[t]=vzL[b];
                Tcx[t]=vxL[c]; Tcy[t]=vyL[c]; Tcz[t]=vzL[c];
                TCX[t]=(Tax[t]+Tbx[t]+Tcx[t])/3.0;
                TCZ[t]=(Taz[t]+Tbz[t]+Tcz[t])/3.0;
            }
            int nSeg=n;
            double[] left=new double[nSeg]; double[] right=new double[nSeg];
            bool[] dead=new bool[nSeg];
            double routeLen=0.0; int deadCount=0;
            for (int s=0;s<nSeg;s++) {
                int s2=(s+1)%n;
                double x0=RX[s], z0=RZ[s], x1=RX[s2], z1=RZ[s2];
                double dx=x1-x0, dz=z1-z0;
                double len=Math.Sqrt(dx*dx+dz*dz);
                routeLen+=len;
                if (len<1e-9) { dead[s]=true; deadCount++; continue; }
                double mx=0.5*(x0+x1), mz=0.5*(z0+z1);
                double nx=-dz/len, nz=dx/len;
                cand.Clear();
                for (int t=0;t<nT;t++) {
                    double dx2=TCX[t]-mx, dz2=TCZ[t]-mz;
                    if (dx2*dx2+dz2*dz2<=900.0) cand.Add(t);
                }
                ys.Clear();
                for (int k=0;k<cand.Count;k++) {
                    double y;
                    if (Hit(cand[k],mx,mz,out y)) ys.Add(y);
                }
                if (ys.Count==0) { dead[s]=true; deadCount++; left[s]=double.NaN; right[s]=double.NaN; continue; }
                ys.Sort(0,ys.Count,null);
                double roadY=ys[ys.Count/2];
                left[s]=Walk(mx,mz,-nx,-nz,roadY,cand);
                right[s]=Walk(mx,mz,nx,nz,roadY,cand);
            }
            var per=new List<double>(nSeg);
            for (int s=0;s<nSeg;s++) if (!dead[s]) per.Add(Math.Min(left[s],right[s]));
            per.Sort();
            double minHalf=per.Count>0?per[0]:double.NaN;
            int argSeg=-1; double argX=double.NaN, argZ=double.NaN;
            for (int s=0;s<nSeg && per.Count>0;s++) {
                if (!dead[s] && Math.Min(left[s],right[s])==minHalf) {
                    argSeg=s; argX=0.5*(RX[s]+RX[(s+1)%n]); argZ=0.5*(RZ[s]+RZ[(s+1)%n]); break;
                }
            }
            double p10=per.Count>0?per[Math.Max(0,(int)Math.Floor(0.1*(per.Count-1)))]:double.NaN;
            double med=per.Count>0?per[per.Count/2]:double.NaN;
            int nB6=0,nB82=0,nSat=0;
            foreach (double v in per) { if (v<6.0) nB6++; if (v<8.2) nB82++; if (v>=21.99) nSat++; }
            string row=idx+","+name+","+map+"_"+layout+","+n+","+nV+","+nT+","+Math.Round(routeLen,1).ToString(ci)+","+nSeg+","+deadCount+","+minHalf.ToString("F2",ci)+","+argSeg+","+argX.ToString("F2",ci)+","+argZ.ToString("F2",ci)+","+p10.ToString("F2",ci)+","+med.ToString("F2",ci)+","+nB6+","+nB82+","+nSat;
            sb.AppendLine(row);
            for (int s=0;s<nSeg;s++) {
                string l=dead[s]?"NA":left[s].ToString("F2",ci);
                string r=dead[s]?"NA":right[s].ToString("F2",ci);
                string ps=dead[s]?"NA":Math.Min(left[s],right[s]).ToString("F2",ci);
                sbSeg.AppendLine(idx+","+s+","+ (0.5*(RX[s]+RX[(s+1)%n])).ToString("F2",ci)+","+ (0.5*(RZ[s]+RZ[(s+1)%n])).ToString("F2",ci)+","+l+","+r+","+ps);
            }
        }
        File.WriteAllText(outCsv,sb.ToString());
        File.WriteAllText(outSegCsv,sbSeg.ToString());
        return "done";
    }
}
'@
Add-Type -TypeDefinition $src -Language CSharp
$r = [CorridorSweep]::Run(
  'C:/Users/shash/MySimGodot-wt-track/_tmp_tracks.txt',
  'C:/Users/shash/Documents/MySimGodot/src/Logic/SourceRoutes.cs',
  'C:/Users/shash/Documents/MySimGodot/assets/Resources',
  'C:/Users/shash/MySimGodot-wt-track/_tmp_sweep.csv',
  'C:/Users/shash/MySimGodot-wt-track/_tmp_sweep_segs.csv')
Write-Output "RESULT: $r"
