param([string]$Root = 'C:\Personal\CemacoApp\presentaciones')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$utf8 = [System.Text.UTF8Encoding]::new($false)
$presType='http://schemas.openxmlformats.org/officeDocument/2006/relationships'
$pkgType='http://schemas.openxmlformats.org/package/2006/relationships'
$a='http://schemas.openxmlformats.org/drawingml/2006/main'
$p='http://schemas.openxmlformats.org/presentationml/2006/main'
$r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'
function Add-Text([System.IO.Compression.ZipArchive]$zip,[string]$name,[string]$value){$entry=$zip.CreateEntry($name,[System.IO.Compression.CompressionLevel]::Optimal);$stream=$entry.Open();try{$bytes=$utf8.GetBytes($value);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}}
function Add-File([System.IO.Compression.ZipArchive]$zip,[string]$name,[string]$source){$entry=$zip.CreateEntry($name,[System.IO.Compression.CompressionLevel]::Optimal);$stream=$entry.Open();$input=[System.IO.File]::OpenRead($source);try{$input.CopyTo($stream)}finally{$input.Dispose();$stream.Dispose()}}
$group='<p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>'
$clrMap='<p:clrMap accent1="accent1" accent2="accent2" accent3="accent3" accent4="accent4" accent5="accent5" accent6="accent6" bg1="lt1" bg2="lt2" folHlink="folHlink" hlink="hlink" tx1="dk1" tx2="dk2"/>'
$colors=@{dk1='173044';lt1='FFFFFF';dk2='103D30';lt2='F7F9F6';accent1='127347';accent2='E7B452';accent3='52665B';accent4='ADCDB6';accent5='7E9F8A';accent6='DCE8DE';hlink='127347';folHlink='52665B'}
$clr=(@('dk1','lt1','dk2','lt2','accent1','accent2','accent3','accent4','accent5','accent6','hlink','folHlink') | ForEach-Object {"<a:${_}><a:srgbClr val=`"$($colors[$_])`"/></a:${_}>"}) -join ''
$fill=('<a:solidFill><a:schemeClr val="accent1"/></a:solidFill>' * 3)
$line=('<a:ln w="9525"><a:solidFill><a:schemeClr val="accent1"/></a:solidFill><a:prstDash val="solid"/></a:ln>' * 3)
$effect=('<a:effectStyle><a:effectLst/></a:effectStyle>' * 3)
$bg=('<a:solidFill><a:schemeClr val="lt1"/></a:solidFill>' * 3)
$theme="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><a:theme xmlns:a=`"$a`" name=`"Cemaco`"><a:themeElements><a:clrScheme name=`"Cemaco`">$clr</a:clrScheme><a:fontScheme name=`"Cemaco`"><a:majorFont><a:latin typeface=`"Arial`"/><a:ea typeface=`"`"/><a:cs typeface=`"`"/></a:majorFont><a:minorFont><a:latin typeface=`"Arial`"/><a:ea typeface=`"`"/><a:cs typeface=`"`"/></a:minorFont></a:fontScheme><a:fmtScheme name=`"Cemaco`"><a:fillStyleLst>$fill</a:fillStyleLst><a:lnStyleLst>$line</a:lnStyleLst><a:effectStyleLst>$effect</a:effectStyleLst><a:bgFillStyleLst>$bg</a:bgFillStyleLst></a:fmtScheme></a:themeElements></a:theme>"
$master="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><p:sldMaster xmlns:a=`"$a`" xmlns:r=`"$r`" xmlns:p=`"$p`"><p:cSld name=`"Blank`"><p:spTree>$group</p:spTree></p:cSld>$clrMap<p:sldLayoutIdLst><p:sldLayoutId id=`"2147483649`" r:id=`"rId1`"/></p:sldLayoutIdLst><p:txStyles><p:titleStyle/><p:bodyStyle/><p:otherStyle/></p:txStyles></p:sldMaster>"
$layout="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><p:sldLayout xmlns:a=`"$a`" xmlns:r=`"$r`" xmlns:p=`"$p`" type=`"blank`" preserve=`"1`"><p:cSld name=`"Blank`"><p:spTree>$group</p:spTree></p:cSld><p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr></p:sldLayout>"
$masterRels="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Relationships xmlns=`"$pkgType`"><Relationship Id=`"rId1`" Type=`"$presType/slideLayout`" Target=`"../slideLayouts/slideLayout1.xml`"/><Relationship Id=`"rId2`" Type=`"$presType/theme`" Target=`"../theme/theme1.xml`"/></Relationships>"
$layoutRels="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Relationships xmlns=`"$pkgType`"><Relationship Id=`"rId1`" Type=`"$presType/slideMaster`" Target=`"../slideMasters/slideMaster1.xml`"/></Relationships>"
$rootRels="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Relationships xmlns=`"$pkgType`"><Relationship Id=`"rId1`" Type=`"$presType/officeDocument`" Target=`"ppt/presentation.xml`"/><Relationship Id=`"rId2`" Type=`"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties`" Target=`"docProps/core.xml`"/><Relationship Id=`"rId3`" Type=`"$presType/extended-properties`" Target=`"docProps/app.xml`"/></Relationships>"
foreach($kind in @('exposicion','respaldo')){
  $images=@(Get-ChildItem -LiteralPath (Join-Path $Root 'rendered') -Filter "$kind-*.png" | Sort-Object Name)
  if($images.Count -eq 0){throw "No hay imágenes para $kind"}
  $destName=if($kind -eq 'exposicion'){'Cemaco_Exposicion_3min.pptx'}else{'Cemaco_Respaldo_Preguntas.pptx'}
  $dest=Join-Path $Root $destName
  $stream=[System.IO.File]::Open($dest,[System.IO.FileMode]::Create,[System.IO.FileAccess]::Write)
  $zip=[System.IO.Compression.ZipArchive]::new($stream,[System.IO.Compression.ZipArchiveMode]::Create,$false)
  try{
    $overrides=@('<Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>','<Override PartName="/ppt/slideMasters/slideMaster1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml"/>','<Override PartName="/ppt/slideLayouts/slideLayout1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml"/>','<Override PartName="/ppt/theme/theme1.xml" ContentType="application/vnd.openxmlformats-officedocument.theme+xml"/>','<Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>','<Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>')
    for($i=1;$i -le $images.Count;$i++){$overrides+="<Override PartName=`"/ppt/slides/slide$i.xml`" ContentType=`"application/vnd.openxmlformats-officedocument.presentationml.slide+xml`"/>"}
    $contentTypes="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Types xmlns=`"http://schemas.openxmlformats.org/package/2006/content-types`"><Default Extension=`"rels`" ContentType=`"application/vnd.openxmlformats-package.relationships+xml`"/><Default Extension=`"xml`" ContentType=`"application/xml`"/><Default Extension=`"png`" ContentType=`"image/png`"/>$($overrides -join '')</Types>"
    Add-Text $zip '[Content_Types].xml' $contentTypes;Add-Text $zip '_rels/.rels' $rootRels
    $ids=for($i=1;$i -le $images.Count;$i++){"<p:sldId id=`"$($i+255)`" r:id=`"rId$($i+1)`"/>"}
    $presentation="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><p:presentation xmlns:a=`"$a`" xmlns:r=`"$r`" xmlns:p=`"$p`"><p:sldMasterIdLst><p:sldMasterId id=`"2147483648`" r:id=`"rId1`"/></p:sldMasterIdLst><p:sldIdLst>$($ids -join '')</p:sldIdLst><p:sldSz cx=`"12192000`" cy=`"6858000`" type=`"screen16x9`"/><p:notesSz cx=`"6858000`" cy=`"9144000`"/><p:defaultTextStyle/></p:presentation>"
    $presRels=@("<Relationship Id=`"rId1`" Type=`"$presType/slideMaster`" Target=`"slideMasters/slideMaster1.xml`"/>")
    for($i=1;$i -le $images.Count;$i++){$presRels+="<Relationship Id=`"rId$($i+1)`" Type=`"$presType/slide`" Target=`"slides/slide$i.xml`"/>"}
    Add-Text $zip 'ppt/presentation.xml' $presentation
    Add-Text $zip 'ppt/_rels/presentation.xml.rels' "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Relationships xmlns=`"$pkgType`">$($presRels -join '')</Relationships>"
    Add-Text $zip 'ppt/slideMasters/slideMaster1.xml' $master;Add-Text $zip 'ppt/slideMasters/_rels/slideMaster1.xml.rels' $masterRels
    Add-Text $zip 'ppt/slideLayouts/slideLayout1.xml' $layout;Add-Text $zip 'ppt/slideLayouts/_rels/slideLayout1.xml.rels' $layoutRels
    Add-Text $zip 'ppt/theme/theme1.xml' $theme
    $now=[DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
    $core="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><cp:coreProperties xmlns:cp=`"http://schemas.openxmlformats.org/package/2006/metadata/core-properties`" xmlns:dc=`"http://purl.org/dc/elements/1.1/`" xmlns:dcterms=`"http://purl.org/dc/terms/`" xmlns:dcmitype=`"http://purl.org/dc/dcmitype/`" xmlns:xsi=`"http://www.w3.org/2001/XMLSchema-instance`"><dc:title>Cemaco $kind</dc:title><dc:creator>Proyecto Grupo 3</dc:creator><dcterms:created xsi:type=`"dcterms:W3CDTF`">$now</dcterms:created></cp:coreProperties>"
    Add-Text $zip 'docProps/core.xml' $core
    Add-Text $zip 'docProps/app.xml' "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Properties xmlns=`"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties`" xmlns:vt=`"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes`"><Application>Cemaco</Application><Slides>$($images.Count)</Slides><PresentationFormat>On-screen Show (16:9)</PresentationFormat></Properties>"
    for($i=1;$i -le $images.Count;$i++){
      $picture="<p:pic><p:nvPicPr><p:cNvPr id=`"2`" name=`"Slide image $i`"/><p:cNvPicPr><a:picLocks noChangeAspect=`"1`"/></p:cNvPicPr><p:nvPr/></p:nvPicPr><p:blipFill><a:blip r:embed=`"rId2`"/><a:stretch><a:fillRect/></a:stretch></p:blipFill><p:spPr><a:xfrm><a:off x=`"0`" y=`"0`"/><a:ext cx=`"12192000`" cy=`"6858000`"/></a:xfrm><a:prstGeom prst=`"rect`"><a:avLst/></a:prstGeom></p:spPr></p:pic>"
      $slide="<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><p:sld xmlns:a=`"$a`" xmlns:r=`"$r`" xmlns:p=`"$p`"><p:cSld><p:spTree>$group$picture</p:spTree></p:cSld><p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr></p:sld>"
      Add-Text $zip "ppt/slides/slide$i.xml" $slide
      Add-Text $zip "ppt/slides/_rels/slide$i.xml.rels" "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?><Relationships xmlns=`"$pkgType`"><Relationship Id=`"rId1`" Type=`"$presType/slideLayout`" Target=`"../slideLayouts/slideLayout1.xml`"/><Relationship Id=`"rId2`" Type=`"$presType/image`" Target=`"../media/image$i.png`"/></Relationships>"
      Add-File $zip "ppt/media/image$i.png" $images[$i-1].FullName
    }
  }finally{$zip.Dispose();$stream.Dispose()}
  Write-Output "$dest : $($images.Count) diapositivas"
}


