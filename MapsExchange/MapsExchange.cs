using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using Map = ExileCore.PoEMemory.Components.Map;

namespace MapsExchange
{
    public class MapsExchange : BaseSettingsPlugin<MapsExchangeSettings>
    {
        private readonly Dictionary<int, float> ArenaEffectiveLevels = new Dictionary<int, float>
        {
            {71, 70.94f},
            {72, 71.82f},
            {73, 72.64f},
            {74, 73.4f},
            {75, 74.1f},
            {76, 74.74f},
            {77, 75.32f},
            {78, 75.84f},
            {79, 76.3f},
            {80, 76.7f},
            {81, 77.04f},
            {82, 77.32f},
            {83, 77.54f},
            {84, 77.7f},
            {85, 77.85f},
            {86, 77.99f}
        };

        public List<string> ShaperMaps = new List<string> { "MapWorldsHydra", "MapWorldsPhoenix", "MapWorldsMinotaur", "MapWorldsChimera" };

        private readonly Color[] _atlasInventLayerColors = new[]
        {
            Color.Gray,
            Color.White,
            Color.Yellow,
            Color.OrangeRed,
            Color.Red,
        };

        private IList<WorldArea> BonusCompletedMaps;
        private Dictionary<string, int> CachedDropLvl = new Dictionary<string, int>();
        private IList<WorldArea> CompletedMaps;
        private long CurrentStashAddr;
        private bool LastVisible;
        private List<MapItem> MapItems = new List<MapItem>();
        private List<String> linesIgnoreMaps = new List<String>();
        private Color[] SelectColors;
        private IList<WorldArea> ShapeUpgradedMaps;
        

        public override bool Initialise()
        {
            #region Colors

            SelectColors = new[]
            {
                Color.Aqua,
                Color.Blue,
                Color.BlueViolet,
                Color.Brown,
                Color.BurlyWood,
                Color.CadetBlue,
                Color.Chartreuse,
                Color.Chocolate,
                Color.Coral,
                Color.CornflowerBlue,
                Color.Cornsilk,
                Color.Crimson,
                Color.Cyan,
                Color.DarkBlue,
                Color.DarkCyan,
                Color.DarkGoldenrod,
                Color.DarkGray,
                Color.DarkGreen,
                Color.DarkKhaki,
                Color.DarkMagenta,
                Color.DarkOliveGreen,
                Color.DarkOrange,
                Color.DarkOrchid,
                Color.DarkRed,
                Color.DarkSalmon,
                Color.DarkSeaGreen,
                Color.DarkSlateBlue,
                Color.DarkSlateGray,
                Color.DarkTurquoise,
                Color.DarkViolet,
                Color.DeepPink,
                Color.DeepSkyBlue,
                Color.DimGray,
                Color.DodgerBlue,
                Color.Firebrick,
                Color.FloralWhite,
                Color.ForestGreen,
                Color.Fuchsia,
                Color.Gainsboro,
                Color.GhostWhite,
                Color.Gold,
                Color.Goldenrod,
                Color.Gray,
                Color.Green,
                Color.GreenYellow,
                Color.Honeydew,
                Color.HotPink,
                Color.IndianRed,
                Color.Indigo,
                Color.Ivory,
                Color.Khaki,
                Color.Lavender,
                Color.LavenderBlush,
                Color.LawnGreen,
                Color.LemonChiffon,
                Color.LightBlue,
                Color.LightCoral,
                Color.LightCyan,
                Color.LightGoldenrodYellow,
                Color.LightGray,
                Color.LightGreen,
                Color.LightPink,
                Color.LightSalmon,
                Color.LightSeaGreen,
                Color.LightSkyBlue,
                Color.LightSlateGray,
                Color.LightSteelBlue,
                Color.LightYellow,
                Color.Lime,
                Color.LimeGreen,
                Color.Linen,
                Color.Magenta,
                Color.Maroon,
                Color.MediumAquamarine,
                Color.MediumBlue,
                Color.MediumOrchid,
                Color.MediumPurple,
                Color.MediumSeaGreen,
                Color.MediumSlateBlue,
                Color.MediumSpringGreen,
                Color.MediumTurquoise,
                Color.MediumVioletRed,
                Color.MidnightBlue,
                Color.MintCream,
                Color.MistyRose,
                Color.Moccasin,
                Color.NavajoWhite,
                Color.Navy,
                Color.OldLace,
                Color.Olive,
                Color.OliveDrab,
                Color.Orange,
                Color.OrangeRed,
                Color.Orchid,
                Color.PaleGoldenrod,
                Color.PaleGreen,
                Color.PaleTurquoise,
                Color.PaleVioletRed,
                Color.PapayaWhip,
                Color.PeachPuff,
                Color.Peru,
                Color.Pink,
                Color.Plum,
                Color.PowderBlue,
                Color.Purple,
                Color.Red,
                Color.RosyBrown,
                Color.RoyalBlue,
                Color.SaddleBrown,
                Color.Salmon,
                Color.SandyBrown,
                Color.SeaGreen,
                Color.SeaShell,
                Color.Sienna,
                Color.Silver,
                Color.SkyBlue,
                Color.SlateBlue,
                Color.SlateGray,
                Color.Snow,
                Color.SpringGreen,
                Color.SteelBlue,
                Color.Tan,
                Color.Teal,
                Color.Thistle,
                Color.Tomato,
                Color.Transparent,
                Color.Turquoise,
                Color.Violet,
                Color.Wheat,
                Color.White,
                Color.WhiteSmoke,
                Color.Yellow,
                Color.YellowGreen
            };

            #endregion

            var initImage = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "ImagesAtlas.png"), false);
            var initImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "AtlasMapCross.png"), false);
            var vaalCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "vaal.png"), false);
            var transmuteImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "transmute.png"), false);
            var augmentImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "augment.png"), false);
            var alcImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "alc.png"), false);
            linesIgnoreMaps = File.ReadAllLines(Path.Combine(DirectoryFullName, "images", "ignoreMaps.txt")).ToList();

            if (!initImage)
                return false;

            Input.RegisterKey(Keys.LControlKey);

            return true;
        }
        static bool IsPointInsideRectangle(RectangleF rectangle, double x, double y)
        {
            return x>= rectangle.Left && x <= rectangle.Right &&
                   y >= rectangle.Top && y <= rectangle.Bottom;
        }
        public override void Render()
        {
            //LogMessage("ignorelist: " + string.Join(", ", linesIgnoreMaps), 5, Color.Red);

            TestAtlasNodes();
            checkMapIgnore();
            DrawPlayerInvMaps();
            DrawNpcInvMaps();
            DrawAtlasMaps();

            var stash = GameController.IngameState.IngameUi.StashElement;

            if (stash.IsVisible)
            {
                var visibleStash = stash.VisibleStash;

                if (visibleStash != null)
                {
                    var items = visibleStash.VisibleInventoryItems;

                    if (items != null)
                    {
                        HiglightExchangeMaps();
                        HiglightAllMaps(items);

                        if (CurrentStashAddr != visibleStash.Address)
                        {
                            CurrentStashAddr = visibleStash.Address;
                            var updateMapsCount = Settings.MapTabNode.Value == stash.IndexVisibleStash;
                            UpdateData(items, updateMapsCount);
                        }
                    }
                    else
                        CurrentStashAddr = -1;
                }
            }
            else
                CurrentStashAddr = -1;
        }
        private void TestAtlasNodes()
        {
            var ingameState = GameController.Game.IngameState;
            var atlasNodes = ingameState.TheGame.Files.AtlasNodes.EntriesList;

            var node = atlasNodes[0];
            //foreach(var node in atlasNodes)
            //{

                
                    var baseAtlasNodeAddress = node.Address;

                    
                    var tier0 = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x51);


                
                    LogMessage(node.Area + " base tier: " + tier0, 5, Color.Red);
                
                
            //}
        }
            private void checkMapIgnore()
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;

            //if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name))
            if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name) && !bonusComp.Any(x => x.Name == GameController.IngameState.Data.CurrentArea.Name))
            {
                Vector2 newInfoPanel2 = new Vector2(822, 722);
                var drawBox2 = new RectangleF(newInfoPanel2.X, newInfoPanel2.Y, 350, 60);
                Graphics.DrawBox(drawBox2, Color.Red, 5);
                Graphics.DrawText("DONT FINISH MAP", newInfoPanel2, Color.White, 30);

            }
        }
            private void DrawAtlasMaps()
        {
            if (!Settings.ShowOnAtlas.Value) return;

            var atlas = GameController.Game.IngameState.IngameUi.Atlas;

            if (LastVisible != atlas.IsVisible || CompletedMaps == null)
            {
                LastVisible = atlas.IsVisible;

                if (LastVisible)
                {
                    CompletedMaps = GameController.Game.IngameState.ServerData.CompletedAreas;
                    BonusCompletedMaps = GameController.Game.IngameState.ServerData.BonusCompletedAreas;
                    ShapeUpgradedMaps = GameController.Game.IngameState.ServerData.ShapedMaps;
                }
            }

            if (!atlas.IsVisible) return;

            var root = atlas.GetChildAtIndex(0);
            var rootPos = new Vector2(root.X, root.Y);
            var scale = root.Scale;

            foreach (var atlasMap in GameController.Files.AtlasNodes.EntriesList)
            {
                var area = atlasMap.Area;
                var mapName = area.Name;

                if (mapName.Contains("Realm")) continue;

                var layer = GameController.Game.IngameState.ServerData.GetAtlasRegionUpgradesByRegion(atlasMap.Area.WorldAreaId);
                //var centerPos = (atlasMap.GetPosByLayer(layer) * 5.69f + rootPos) * scale;
                //var textRect = centerPos;
                //textRect.Y -= 30 * scale;
                var testSize = (int)Math.Round(Settings.TextSize.Value * scale);
                var fontFlags = FontAlign.Center;

                var tier = atlasMap.GetTierByLayer(layer);
                var areaLvl = 65 + tier;

                byte textTransp;
                Color textBgColor;
                bool fill;
                Color fillColor;

                if (BonusCompletedMaps.Contains(area))
                {
                    textTransp = Settings.BonusCompletedTextTransparency.Value;
                    textBgColor = Settings.BonusCompletedTextBg.Value;
                    fill = Settings.BonusCompletedFilledCircle.Value;
                    fillColor = Settings.BonusCompletedFillColor.Value;
                }
                else if (CompletedMaps.Contains(area))
                {
                    textTransp = Settings.CompletedTextTransparency.Value;
                    textBgColor = Settings.CompletedTextBg.Value;
                    fill = Settings.CompletedFilledCircle.Value;
                    fillColor = Settings.CompletedFillColor.Value;
                }
                else
                {
                    textTransp = Settings.UnCompletedTextTransparency.Value;
                    textBgColor = Settings.UnCompletedTextBg.Value;
                    fill = Settings.UnCompletedFilledCircle.Value;
                    fillColor = Settings.UnCompletedFillColor.Value;
                }

                var textColor = Settings.WhiteMapColor.Value;

                if (areaLvl >= 78)
                    textColor = Settings.RedMapColor.Value;
                else if (areaLvl >= 73)
                    textColor = Settings.YellowMapColor.Value;

                textColor.A = textTransp;

                //Graphics.DrawText(mapName, textRect.Translate(0, -15), textColor, testSize, fontFlags);

                var mapNameSize = Graphics.MeasureText(mapName, testSize);
                mapNameSize.X += 5;

                //var nameBoxRect = new RectangleF(textRect.X - mapNameSize.X / 2, textRect.Y - mapNameSize.Y, mapNameSize.X,
                                                // mapNameSize.Y);

                //Graphics.DrawBox(nameBoxRect, textBgColor);

                if (Input.IsKeyDown(Keys.LControlKey))
                {
                    var upgraded = ShapeUpgradedMaps.Contains(area);
                    var areaLvlColor = Color.White;

                    if (upgraded)
                    {
                        areaLvl += 5;
                        areaLvlColor = Color.Orange;
                    }

                    var penalty = LevelXpPenalty(areaLvl);
                    var penaltyTextColor = Color.Lerp(Color.Red, Color.Green, (float)penalty);
                    var labelText = $"{penalty:p0}";
                    var textSize = Graphics.MeasureText(labelText, testSize);
                    textSize.X += 6;
                   // var penaltyRect = new RectangleF(textRect.X + mapNameSize.X / 2, textRect.Y - textSize.Y, textSize.X, textSize.Y);
                    //Graphics.DrawBox(penaltyRect, Color.Black);
                    //Graphics.DrawText(labelText, penaltyRect.Center.Translate(0, -8), penaltyTextColor, testSize, FontAlign.Center);

                    labelText = $"{areaLvl}";
                    textSize = Graphics.MeasureText(labelText, testSize);

                    //penaltyRect = new RectangleF(textRect.X - mapNameSize.X / 2 - textSize.X, textRect.Y - textSize.Y, textSize.X,
                                             //    textSize.Y);

                    //Graphics.DrawBox(penaltyRect, Color.Black);
                    //Graphics.DrawText(labelText, penaltyRect.Center.Translate(3, -8), areaLvlColor, testSize, FontAlign.Center);

                    if (Settings.ShowBuyButton.Value)
                    {
                        var butTextWidth = 50 * scale;
                       // var buyButtonRect = new RectangleF(textRect.X - butTextWidth / 2, textRect.Y - testSize * 2, butTextWidth, testSize);

                        //Graphics.DrawImage("ImagesAtlas.png", buyButtonRect, new RectangleF(.367f, .731f, .184f, .223f),
                                          // new Color(255, 255, 255, 255));

                        //buyButtonRect
                        //ImGui.Button()
                        //TradeProcessor.OpenBuyMap(BuyAtlasNode.Area.Name, IsUniq(BuyAtlasNode), GameController.Game.IngameState.ServerData.League);
                    }
                }

                var imgRectSize = 60 * scale;
                //var imgDrawRect = new RectangleF(centerPos.X - imgRectSize / 2, centerPos.Y - imgRectSize / 2, imgRectSize, imgRectSize);

                if (fill)
                   // Graphics.DrawImage("ImagesAtlas.png", imgDrawRect, new RectangleF(.5f, 0, .5f, .731f), fillColor);

               // Graphics.DrawImage("ImagesAtlas.png", imgDrawRect, new RectangleF(0, 0, .5f, .731f), Color.Black);

                if (Settings.ShowAmount.Value)
                {
                    if (atlasMap.Area.IsUnique)
                        mapName += ":Uniq";

                    mapName += $":{tier}";

                    if (Settings.MapStashAmount.TryGetValue(mapName, out var amount))
                    {
                        var mapCountSize = Graphics.MeasureText(amount.ToString(), testSize);
                        mapCountSize.X += 6;
                        mapCountSize.Y += 7;

                       // Graphics.DrawBox(
                            //new RectangleF(centerPos.X - mapCountSize.X / 2, centerPos.Y - mapCountSize.Y / 2, mapCountSize.X,
                       //                    mapCountSize.Y), Color.Black);

                        textColor.A = 255;
                        //Graphics.DrawText(amount.ToString(), centerPos.Translate(0, -5), textColor, testSize, FontAlign.Center);
                    }
                }
            }

            DrawAtlasRegionMaps();
        }

        private void DrawAtlasRegionMaps()
        {
            foreach (var keyValuePair in GameController.Files.AtlasRegions.RegionIndexDictionary)
            {
                DrawRegionAmount(keyValuePair.Key, keyValuePair.Value.Name);
            }
        }

        private void DrawRegionAmount(int atlasInvSlot, string regionName)
        {
            var drawPos = GameController.Game.IngameState.IngameUi.Atlas.InventorySlots[atlasInvSlot].GetClientRectCache
                                        .TopRight;

            if (Settings.MapRegionsAmount.TryGetValue(regionName, out var maps))
            {
                for (var i = 0; i < maps.Length; i++)
                {
                    var amount = maps[i];

                    Graphics.DrawText($"L{i}: {amount}", drawPos, _atlasInventLayerColors[i]);
                    drawPos.Y += 15;
                }
            }
        }

        private void DrawPlayerInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
                HiglightAllMaps(inventoryZone);
            }
        }

        private void DrawNpcInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
           
            foreach (var inv in npcInv)
            {
                
                if (inv.Inventory.Rows == 1)
                {//kirac mission++
                    if(GameController.Game.IngameState.IngameUi.ZanaMissionChoice.IsVisible)
                    { 

                        var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;
                        //LogMessage("Entrei");
                        //LogMessage("Children Count: " + KiracPanel.ChildCount.ToString());
                        var inventory = KiracPanel.GetChildFromIndices(0, 3,0,0);
                        //LogMessage("Children indice 0 : " + KiracPanel.GetChildFromIndices(0).ChildCount.ToString());
                        //LogMessage("Children indice 0,3 : " + inventory.ChildCount.ToString());

                        var auxMap = KiracPanel?.GetChildFromIndices(0, 3);
                        var auxMap2 = KiracPanel?.GetChildFromIndices(0, 3,0);
                        var auxMap3 = KiracPanel?.GetChildFromIndices(0, 3,0,0);


                        //LogMessage("Test 03 : " + auxMap[0]?.Item?.GetComponent<Map>()?.Area);
                        //LogMessage("Test 030 : " + auxMap2[0]?.Item?.GetComponent<Map>()?.Area);
                        //LogMessage("Test 0300 : " + auxMap3[0]?.Item?.GetComponent<Map>()?.Area);


                        //var baseX = auxMap.GetClientRect().TopLeft.X;
                        //var baseY = auxMap.GetClientRect().TopLeft.Y;
                        //var baseWidth = auxMap.GetClientRect().Width;
                        //var baseHeight = auxMap.GetClientRect().Height;

                        //var zanaChildren = auxMap.GetChildren<T>().Skip(1).ToList() ?? new List<Element>();
                        var zanaMisionAux = auxMap.GetChildrenAs<ExileCore.PoEMemory.Element>().ToList() ?? new List<ExileCore.PoEMemory.Element>();
                        var firstVisible = zanaMisionAux.First(x => x.IsVisible);
                        var lastVisible = zanaMisionAux.Last(x => x.IsVisible);
                        var firstVisibleIndex = firstVisible.IndexInParent;
                        var lastVisibleIndex = lastVisible.IndexInParent;
                        //LogMessage("firstVisibleIndex " + firstVisibleIndex.ToString(), 5, Color.Red);
                        //LogMessage("lastVisibleIndex " + lastVisibleIndex.ToString(), 5, Color.Red);
                        //foreach (var item in itemList)
                        foreach (var item in inv.Inventory.InventorySlotItems)
                        {
                        
                            var mapComponent = item.Item.GetComponent<Map>();
                            var mapGridX = item.PosX;
                            //var mapGridY = item.PosY;
                            //var mapCenterX = baseX + (baseWidth / 2) + (mapGridX * baseWidth);
                            //var mapCenterY = baseY + (baseHeight / 2) + (mapGridY * baseHeight);
                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.Area;

                            //var shaper = shapered.Contains(mapArea);




                            //-----------

                            var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;

                            //var shaper = shapered.Contains(mapArea);


                            //check item quality, if unique mapcontains name9

                            if (mapRarity != ItemRarity.Unique)
                            {



                                if (bonusComp.Contains(mapArea)) continue; // check item quality
                            }
                            else

                            {
                                var mapUniqueName = item.Item.GetComponent<Mods>().UniqueName;

                                if (bonusComp.Any(r => r.Name == mapUniqueName)) continue;

                            }

                            ///-


                            

                            var color = Color.White;

                            if (mapComponent.Tier > 10)
                            {
                                color = Color.Red;
                            }else if (mapComponent.Tier > 5)
                            {
                                color = Color.Yellow;
                            }


                            var ignoreCompletion = false;

                            if (linesIgnoreMaps.Contains(mapArea.ToString()))
                            {
                                ignoreCompletion = true;

                            }

                            //LogMessage("Map: " + mapArea + " indice x:" + item.InventoryPosition.X + " indice y:" + item.InventoryPosition.Y);
                            var auxindex = (int)item.InventoryPosition.X;
                            var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();

                            var stringtoDraw = mapArea.Name;
                            if (ignoreCompletion)
                                stringtoDraw += " --- IGNORED MAP";
                            Graphics.DrawText(stringtoDraw, drawListPos, color, 20);
                            drawListPos.Y += 20;
                            if (mapGridX < firstVisibleIndex || mapGridX > lastVisibleIndex) continue;
                            //LogMessage("map " + mapArea.ToString() + drawRect2.ToString(), 5, Color.Red);

                            Graphics.DrawFrame(drawRect2, Color.Red, 5);
                            if (ignoreCompletion)
                                Graphics.DrawImage("AtlasMapCross.png", drawRect2, new RectangleF(1, 1, 1, 1), Color.Red);
                            
                            // if map name is in not complete list then
                            
                        }

                    }
                }
                else
                { 
                    foreach (var item in inv.Inventory.InventorySlotItems)
                    {
                        var mapComponent = item.Item.GetComponent<Map>();

                        if (mapComponent == null)
                            continue;
                   
                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X - 961.0f;
                        drawRect.Y = drawRect.Y - 325.0f;

                        var mapArea = mapComponent.Area;
                   
                        //var shaper = shapered.Contains(mapArea);

                        if (bonusComp.Contains(mapArea)) continue;

                        var color = Color.White;

                        if (mapComponent.Tier > 10)
                        {
                            color = Color.Red;
                        }
                        else if (mapComponent.Tier > 5)
                        {
                            color = Color.Yellow;
                        }
                        //LogMessage("Map: " + mapArea);
                       

                        //LogMessage("Map: " + mapArea + " indice x:" + item.InventoryPosition.X + " indice y:" + item.InventoryPosition.Y);
                        var ignoreCompletion = false;

                        if (linesIgnoreMaps.Contains(mapArea.ToString()))
                        {
                            ignoreCompletion = true;

                        }
                        var stringtoDraw = mapArea.Name;
                        if (ignoreCompletion)
                            stringtoDraw += " --- IGNORED MAP";
                        Graphics.DrawText(mapArea.Name, drawListPos, color, 20);
                        Graphics.DrawFrame(drawRect, Color.Red, 5);
                        if (ignoreCompletion)
                            Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Color.Red);
                        drawListPos.Y += 20;
                    }
                }
            }

            /*
          if (ingameState.IngameUi.InventoryPanel.IsVisible)
          {
              List<NormalInventoryItem> playerInvItems = new List<NormalInventoryItem>();
              var inventoryZone = ingameState.IngameUi.InventoryPanel[PoeHUD.Models.Enums.InventoryIndex.PlayerInventory].VisibleInventoryItems;//.InventoryUiElement;
              foreach (Element element in inventoryZone.Children)
              {
                  var inventElement = element.AsObject<NormalInventoryItem>();
                  if (inventElement.InventPosX < 0 || inventElement.InventPosY < 0)
                  {
                      continue;
                  }
                  playerInvItems.Add(inventElement);
              }
            
            HiglightAllMaps(playerInvItems);
            }
              */
        }

        private void UpdateData(IList<NormalInventoryItem> items, bool checkAmount)
        {
            MapItems = new List<MapItem>();

            if (checkAmount)
            {
                Settings.MapStashAmount.Clear();
                Settings.MapRegionsAmount.Clear();
            }
            var mapStashAmount = new Dictionary<string, int>();
            var mapRegionsAmount = new Dictionary<string, int[]>();
       
            foreach (var invItem in items)
            {
                var item = invItem.Item;

                if (item == null) continue;

                var bit = GameController.Files.BaseItemTypes.Translate(item.Path);

                if (bit == null) continue;

                if (bit.ClassName != "Map") continue;

                float width = Settings.BordersWidth;
                float spacing = Settings.Spacing;

                var drawRect = invItem.GetClientRect();
                drawRect.X += width / 2 + spacing;
                drawRect.Y += width / 2 + spacing;
                drawRect.Width -= width + spacing * 2;
                drawRect.Height -= width + spacing * 2;

                var baseName = bit.BaseName;
                var map = item.GetComponent<Map>();

                if (map == null) continue;

                var mapItem = new MapItem(baseName, drawRect, map.Tier);
                var mapComponent = item.GetComponent<Map>();

                if (mapComponent == null) continue;

                if (checkAmount)
                {
                    var area = mapComponent.Area;

                    if (area == null)
                    {
                        LogError($"Area is null on {item.Address:X} {item.Path}", 3);

                        continue;
                    }

                    var areaName = area.Name;
                    var mods = item.GetComponent<Mods>();

                    if (mods.ItemRarity == ItemRarity.Unique)
                    {
                        areaName = mods.UniqueName.Replace(" Map", string.Empty);
                        areaName += ":Uniq";
                    }

                    areaName += $":{mapComponent.Tier}";

                    var nodes = GameController.Files.AtlasNodes.EntriesList
                                              .Where(x => x.Area.Id == area.Id).ToList();

                    var node = nodes.FirstOrDefault();

                    if (node == null)
                    {
                        //LogError($"Cannot find AtlasNode for area {mapComponent.Area}");
                    }
                    else
                    {
                        var layerIndex = node.GetLayerByTier(mapComponent.Tier);

                        if (layerIndex != -1 && layerIndex < 5)
                        {
                            //if (!mapRegionsAmount.TryGetValue(node.AtlasRegion.Name, out var list))
                            //{
                                //list = new int[5];
                               //mapRegionsAmount[node.AtlasRegion.Name] = list;
                            //}

                            //list[layerIndex]++;
                        }
                        else
                        {
                            //LogError($"Cannot find layer for area {mapComponent.Area} with tier {mapComponent.Tier}. Layer result: {layerIndex}");
                        }
                    }

                    if (!mapStashAmount.ContainsKey(areaName))
                        mapStashAmount.Add(areaName, 1);
                    else
                        mapStashAmount[areaName]++;
                }

                mapItem.Penalty = LevelXpPenalty(TierToLevel(mapComponent.Tier));
                MapItems.Add(mapItem);
            }

            var stashElement = GameController.IngameState.IngameUi.StashElement;

            if (checkAmount && stashElement.IsVisible /*&& stashElement.VisibleStash?.VisibleInventoryItems != null*/)
            {
                Settings.MapStashAmount = mapStashAmount;
                Settings.MapRegionsAmount = mapRegionsAmount;
            }

            var sortedMaps = (from demoClass in MapItems
                              //where demoClass.Tier >= Settings.MinTier && demoClass.Tier <= Settings.MaxTier
                              group demoClass by $"{demoClass.Name}|{demoClass.Tier}"
                              into groupedDemoClass
                              select groupedDemoClass
                ).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

            var colorCounter = 0;

            foreach (var group in sortedMaps)
            {
                var count = group.Value.Count;
                var take = count / 3;
                take *= 3;

                var grabMaps = group.Value.Take(take);

                foreach (var dropMap in grabMaps)
                {
                    dropMap.DrawColor = SelectColors[colorCounter];
                }

                colorCounter = ++colorCounter % SelectColors.Length;
            }
        }

        private int TierToLevel(int tier)
        {
            return 65 + tier;
        }

        private void HiglightExchangeMaps()
        {
            if (!Settings.ShowExchange.Value)
                return;

            foreach (var drapMap in MapItems)
            {
                Graphics.DrawFrame(drapMap.DrawRect, drapMap.DrawColor, Settings.BordersWidth.Value);
            }
        }

        private void HiglightAllMaps(IList<NormalInventoryItem> items)
        {
           // LogMessage("Entrou", 5, Color.Red);
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shEld = serverData.ShaperElderAreas;

            var disableOnHover = false;
            var disableOnHoverRect = new RectangleF();
            //LogMessage("Entrou 1", 5, Color.Red);
            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();

            var tooltip = inventoryItemIcon?.Tooltip;
                        //LogMessage("Entrou 1", 5, Color.Red);
            if (tooltip != null)
            {
                disableOnHover = true;
                disableOnHoverRect = tooltip.GetClientRect();
            }

            foreach (var item in items)
            {
                //LogMessage("Entrou 2", 5, Color.Red);
                var entity = item?.Item;

                if (entity == null) continue;
                //LogMessage("Entrou 3", 5, Color.Red);
                var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);

                if (bit == null) continue;
                //LogMessage("Entrou 4", 5, Color.Red);
                if (bit.ClassName != "Map" && bit.ClassName != "Maps") continue;
                //LogMessage("Entrou 5", 5, Color.Red);
                var mapComponent = entity.GetComponent<Map>();
                var modsComponent = entity.GetComponent<Mods>();
                var baseComponent = entity.GetComponent<Base>();
                
                var rarity = modsComponent.ItemRarity;
                var corrupted = baseComponent.isCorrupted;

                var drawRect = item.GetClientRect();
                var drawRect2 = item.GetClientRect();

                if (disableOnHover && disableOnHoverRect.Intersects(drawRect))
                    continue;
                //LogMessage("Entrou 6", 5, Color.Red);
                var offset = 3;
                drawRect.Top += offset;
                drawRect.Bottom -= offset;
                drawRect.Right -= offset;
                drawRect.Left += offset;

                var offset2 = 1;
                //drawRect2.Top += offset2 + offset2;
                //drawRect2.Bottom -= offset2 + offset2;
                //drawRect2.Right -= offset2 + offset2;
                //drawRect2.Left += offset2 + offset2;

                drawRect2.Top += 20;
                drawRect2.Bottom -= 0;
                drawRect2.Right -= 0;
                drawRect2.Left += 20;

                var completed = 0;

                var area = mapComponent.Area;
                var tier = mapComponent.Tier;
                var mapMods = modsComponent.ExplicitMods.Count();

                //if (comp.Contains(area))
                //    completed++;

                if (bonusComp.Contains(area) || ShaperMaps.Contains(area.Id))
                    completed++;

                var shaperElder = shEld.Contains(area);
				if (linesIgnoreMaps.Contains(area.ToString()))
                    {
                        Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Color.Red);

                    }


                //MapWorldsMinotaur
                //MapWorldsHydra
                //MapWorldsPhoenix
                //MapWorldsChimera

                if (completed == 0)
                {
                    
                    

                    if( tier > 10 && (rarity<ItemRarity.Rare || !corrupted) && rarity!=ItemRarity.Unique )
                    {
                        // mostrar vaal orb.
                        Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Yellow);
                        Graphics.DrawImage("vaal.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier > 5 && rarity < ItemRarity.Rare && rarity != ItemRarity.Unique)
                    {
                        // mostrar alc orb
                        Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Yellow);
                        Graphics.DrawImage("alc.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier <= 5 && rarity < ItemRarity.Magic && rarity != ItemRarity.Unique)
                    {
                        // mostrar trans orb
                        Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Yellow);
                        Graphics.DrawImage("transmute.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier <= 5 && rarity == ItemRarity.Magic && rarity != ItemRarity.Unique && mapMods < 2)
                    {
                        // mostrar trans orb
                        Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.Yellow);
                        Graphics.DrawImage("augment.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else
                    {
                        Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Color.LightGreen);
                    }


                    // if map name is in not complete list then
                   


                }
                var rectAux = drawRect;
                rectAux.Width = 20;
                rectAux.Height = 10;
                Graphics.DrawBox(rectAux, Color.Black,5);

				Graphics.DrawText(tier.ToString(), new Vector2(drawRect.X - 2, drawRect.Y - 2), Color.White);
  

               
            }
        }

        private double LevelXpPenalty(int arenaLevel)
        {
            var characterLevel = GameController.Player.GetComponent<Player>().Level;

            float effectiveArenaLevel;

            if (arenaLevel < 71)
                effectiveArenaLevel = arenaLevel;
            else
            {
                if (!ArenaEffectiveLevels.TryGetValue(arenaLevel, out var scale))
                {
                    LogError($"Can't calc ArenaEffectiveLevels from arenaLevel: {arenaLevel}", 2);

                    return 0;
                }

                effectiveArenaLevel = scale;
            }

            var safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            var effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95) //For player levels equal to or higher than 95:
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);

            return xpMultiplier;
        }

        public class MapItem
        {
            public Color DrawColor = Color.Transparent;
            public RectangleF DrawRect;
            public string Name;
            public double Penalty;
            public int Tier { get; }

            public MapItem(string Name, RectangleF DrawRect, int tier)
            {
                this.Name = Name;
                this.DrawRect = DrawRect;
                Tier = tier;
            }
        }
    }
}
