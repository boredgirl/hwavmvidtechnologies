using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Hwavmvid.Motorsport.Shared.Items;

namespace Hwavmvid.Motorsport.Racewaymaps
{
    public class Motorsportracewaymapcreatorbase : ComponentBase, IDisposable
    {

        [Inject] public Motorsportracewayservice Motorsportracewayservice { get; set; }

        public bool loading { get; set; }

        public const double containerwidth = rows * griditemwidth;
        public const double containerheight = cols * griditemheight;

        public const double griditemwidth = 80;
        public const double griditemheight = 80;

        private const int rows = 10;
        private const int cols = 10;

        public string Platformcolor { get; set; } = ConsoleColor.DarkGray.ToString(); // #216d46

        protected override Task OnInitializedAsync()
        {

            this.Motorsportracewayservice.OnUpdateComponent += UpdateUI;

            this.Motorsportracewayservice.Map = this.GetMap();
            this.Initplatformitems();
            this.Motorsportracewayservice.Createdraggableitems();

            return base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await this.Motorsportracewayservice.InitMotorsportracewayService();
            await base.OnAfterRenderAsync(firstRender);
        }

        public MRacewaymap GetMap()
        {

            MRacewaymap map = new MRacewaymap();
            for (var r = 1; r <= rows; r++)
            {

                MRacewayrow row = new MRacewayrow();
                row.RowId = r;
                map.Rows.Add(row);

                for (var c = 1; c <= cols; c++)
                {

                    MRacewaycolumn column = new MRacewaycolumn();
                    column.ColumnId = c;
                    column.RowId = r;
                    map.Columns.Add(column);
                }
            }

            return map;
        }
        public MRacewaymapitem<MRacewayitemtype> platformitemtemplate { get; set; }
        public void Initplatformitems()
        {

            foreach (var row in Motorsportracewayservice.Map.Rows)
            {

                foreach (var container in Motorsportracewayservice.Map.Columns.Where(item => item.RowId == row.RowId).Select((item, index) => new { item = item, index = index }))
                {

                    this.platformitemtemplate = new MRacewaymapitem<MRacewayitemtype>(Guid.NewGuid().ToString(), MRacewayitemtype.Platform);
                    this.platformitemtemplate.RowId = row.RowId;
                    this.platformitemtemplate.ColumnId = container.index + 1;
                    this.platformitemtemplate.ZIndex = 1;
                    this.platformitemtemplate.Opacity = 1;
                    this.platformitemtemplate.BackgroundColor = this.Platformcolor;
                    this.platformitemtemplate.Rotation = 0;
                    this.platformitemtemplate.ImageWidth = 0;
                    this.platformitemtemplate.ImageHeight = 0;
                    this.platformitemtemplate.ImageUrl = string.Empty;
                    this.platformitemtemplate.ImageUrlExtension = string.Empty;
                    this.platformitemtemplate.Value = 0;

                    this.Motorsportracewayservice.AddMapColumnItem(platformitemtemplate.RowId, platformitemtemplate.ColumnId, this.platformitemtemplate);
                }
            }
        }

        public async void UpdateUI()
        {
            await InvokeAsync(() =>
            {
                this.StateHasChanged();
            });
        }
        public void Dispose()
        {
            this.Motorsportracewayservice.UpdateUI -= UpdateUI;
        }

    }
}
