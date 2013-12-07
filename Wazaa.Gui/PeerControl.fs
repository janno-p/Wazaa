module Wazaa.Gui.PeerControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Config

type PeerControl() as this =
    inherit UserControl()

    let addButton = new Button(Text = "Add")
    let editButton = new Button(Text = "Edit", Enabled = false)
    let removeButton = new Button(Text = "Remove", Enabled = false)

    let add c =
        this.Controls.Add(c)

    do [addButton; editButton; removeButton] |> Seq.iteri (fun i x ->
        add x
        x.Size <- new Size(75, 23)
        x.Left <- this.Width - 75
        x.Top <- 26 * i
        x.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
    )

    let peerListView =
        let listView = new ListView()
        listView.Anchor <- AnchorStyles.Left ||| AnchorStyles.Top ||| AnchorStyles.Bottom ||| AnchorStyles.Right
        listView.Size <- new Size(this.Width - 78, this.Height)
        listView.MultiSelect <- false
        listView.View <- View.Details
        listView.FullRowSelect <- true
        listView.HideSelection <- false
        [ "IP Address"; "Port" ] |> Seq.iter (fun str -> listView.Columns.Add(str, -1) |> ignore)
        listView.SelectedIndexChanged.AddHandler (fun sender args ->
            let enabled = listView.SelectedItems.Count > 0
            [editButton; removeButton] |> Seq.iter (fun btn -> btn.Enabled <- enabled)
            )
        KnownPeers |> Seq.iter (fun peer ->
            let item = new ListViewItem([| peer.Address.ToString(); peer.Port.ToString() |], Tag = peer)
            listView.Items.Add(item) |> ignore
            )
        add listView
        listView

    do this.Dock <- DockStyle.Fill
    do this.Padding <- new Padding(3)
