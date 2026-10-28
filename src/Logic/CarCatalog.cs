namespace MySimulator.Rebuild {
 // Original RR3 vehicles shipped as Resources/RealRacing/Car{i}.
 public static class CarCatalog{
  public static readonly string[] Folders={"2018_ferrari_488_gt3","2018_audi_r8_gt4","2018_bmw_m4_gt4","2018_chevrolet_camaro_gt4r","2018_bmw_m8_gte","2018_chevrolet_camaro_zl1_1le",
   "2018_lamborghini_huracan_gt3","2016_mercedes_amg_gt3","2015_mclaren_650s_gt3","2018_acura_nsx_gt3","2018_bmw_m6_gt3","2018_porsche_911_rsr","2016_ford_gt_lm","2018_astonmartin_gte","2016_chevrolet_c7r",
   "2019_f1_mercedes_w10","2019_f1_ferrari_sf90","2019_f1_red_bull_rb15","2019_f1_mclaren_mcl34","2015_mclaren_p1_gtr","2017_bugatti_chiron","2018_porsche_919_hybrid_evo","2017_oreca_07_lmp2","2018_porsche_911_gt3_rs"};
  public static readonly string[] Names={"FERRARI 488 GT3","AUDI R8 GT4","BMW M4 GT4","CAMARO GT4.R","BMW M8 GTE","CAMARO ZL1 1LE",
   "HURACAN GT3","MERCEDES-AMG GT3","McLAREN 650S GT3","ACURA NSX GT3","BMW M6 GT3","PORSCHE 911 RSR","FORD GT LM","ASTON MARTIN GTE","CORVETTE C7.R",
   "MERCEDES W10","FERRARI SF90","RED BULL RB15","McLAREN MCL34","McLAREN P1 GTR","BUGATTI CHIRON","PORSCHE 919 EVO","ORECA 07 LMP2","PORSCHE 911 GT3 RS"};
  public static int Count{get{return Folders.Length;}}
 }}
