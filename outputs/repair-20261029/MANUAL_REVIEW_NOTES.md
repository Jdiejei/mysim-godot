# Manual visual review — first source texture batch

Evidence: cloud assets-part-00000.enc sheets textures-0000 through textures-0003; thumbnails 0–399 were inspected. Per-item paths and tile indices are in MANUAL_REVIEW_LEDGER.json. This is overview review, not full-resolution/UV/in-game acceptance. No mesh is marked manually reviewed yet.

- Branding atlases at 16–30, 126–140 and 304–318 contain literal AAA/BBB/CCC sponsor-slot placeholders. The runtime replacement must cover every material using them; these must not reappear when importing candidates. Keep the underlying source atlas and UV assignment; do not paint over shared production originals.
- Directional sign/font textures (113, 166, 273–274, 394–395) appear inverted/mirrored in the atlas. This alone is not a texture defect: require the source UV transform and an in-game facing check before changing either texture or mesh UVs. Avoid a global image flip.
- Foliage tiles include disconnected branch/trunk and canopy regions, transparent margins and multiple variants. Preserve alpha, atlas layout and local UVs; use alpha clipping and validate mip-edge halos and front/back visibility in rendered scenes. Solid grey in these sheets is the review compositor, not proof of baked grey foliage.
- The asphalt/concrete/curb families contain distinct authored surfaces and localized overlay assets. Preserve local wear and skid decals where intentional, while preventing repetitive full-road streaks. Do not treat every dark/flat-looking thumbnail as corrupted content.
- Vehicle service props use unfolded body/wheel atlases. Judge them on their source geometry rather than converting atlas pieces into standalone textures or assigning the entire atlas to unrelated meshes.
- Repeated Brands Hatch texture families across later tiles indicate reuse/duplicates requiring hash and binding comparison. Reuse is not a reason to delete a source or accept a candidate.

Next: review texture tiles 400 onward, and the newly converted candidate meshes with their four labelled views. There are 4,263 textures in this first transport part alone; remaining parts are additional scope.

## Candidate mesh overview, first 75 objects

Cloud mesh-part-00000 sheets 0000?0002 were inspected (300 views). These are separate source components, not finished cars or complete track assemblies. Many are thin barriers, signs, vegetation cards, walls, roofs or equipment parts; some car-category records are visibly trackside assets. Reject the assumption that importing every file automatically upgrades the game. Several previews show dark foliage cards, featureless white panels, disconnected component sets and clipped/overexposed detail views. Preserve originals and require assembled context and PBR/alpha/UV checks. No candidate is accepted from these thumbnails. The GLB conversion is a staging result, not a visual-quality pass.

Mesh sheets 0003?0005 were also inspected: another 75 objects / 300 views, bringing this part to 150 object overviews. Foliage cards still require alpha/backface checks; sign/panel views include featureless or overexposed faces; wheels/vent equipment and vehicle panels are separate assembly components. No assembled-scene or final quality acceptance.

Texture sheets 0004?0006 inspected (400?699). Additional literal branding placeholders are present at 412?420 and 665?681. Painted runoff textures contain deliberate tyre wear; avoid removing it wholesale. Start lights, flags, brake markers, track names and sponsor lettering have directional UV requirements. Fence wire, crowd and foliage atlases require clipped alpha and mip/coverage checks in assembled scenes. Geometry collision must correspond to barriers, not the transparent parts of spectator/foliage cards. These thumbnail observations are not full-resolution acceptance.

Texture sheets 0007?0010 inspected, tiles 700?1099. Hockenheim/Hong Kong/Indianapolis families include authored tunnel/interior darkness, real directional sponsor/type atlases and repeated placeholders (840?855, 1059?1074). Keep authentic dark surfaces distinct from missing materials. Glass, wire fence and crowd atlases require correct transparency rather than uniformly opaque or alpha-clipped assignment. Directional runway/road markings and traffic lights must remain aligned to source UVs. No assembled visual acceptance from thumbnails.

### Texture sheets 0011?0012, indices 1100?1299
Viewed both full sheets. Cedar/oak foliage atlases, crowd cards and wire fences require source alpha and sidedness checks; gray backgrounds in the composited sheet do not prove broken alpha. Laguna Seca sponsor text is upside down in the source atlas: verify assembled UVs rather than globally flipping lettering. Repeated AAA/BBB/CCC billboards remain placeholders, not finished branding. Asphalt swatches are intrinsically dark; the inspected runtime road is darker still, requiring the pending material/shadow comparison. Fine wire/cable textures need distance aliasing inspection. No full-resolution or assembled acceptance.

### Runtime render review
Viewed preview shard5 mosaic (tracks 05,11,17,23,29,35,41,47,53,59) and track05 full resolution. Source 1f3e3cc, older than current wheel/material fixes. Roads nearly black on most layouts; HUD text lacks a stable backdrop and driving buttons blend into dark asphalt. Added opaque HUD strip and explicit bordered button styles; cloud compilation and current-source visual inspection required. These screenshots are not game acceptance.
