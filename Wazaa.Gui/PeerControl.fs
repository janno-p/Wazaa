module Wazaa.Gui.PeerControl

open System.Drawing
open System.Windows.Forms
open Wazaa.Config

type PeerControl() as this =
    inherit UserControl()

    let peerListBox = new ListBox()
    let addButton = new Button(Text = "Add")
    let editButton = new Button(Text = "Edit", Enabled = false)
    let removeButton = new Button(Text = "Remove", Enabled = false)

    let add c =
        this.Controls.Add(c)

    do [addButton; editButton; removeButton] |> Seq.iteri (fun i x ->
        add x
        x.Size <- new Size(75, 23)
        x.Left <- this.Width - 78
        x.Top <- 3 + 26 * i
        x.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
    )

    do peerListBox.Anchor <- AnchorStyles.Left ||| AnchorStyles.Top ||| AnchorStyles.Bottom ||| AnchorStyles.Right
    do peerListBox.Left <- 3
    do peerListBox.Width <- this.Width - 84
    do peerListBox.Top <- 3
    do peerListBox.Height <- this.Height - 6

    do peerListBox.SelectedIndexChanged.AddHandler(fun o e ->
        let enabled = not(peerListBox.SelectedItem = null)
        [editButton; removeButton] |> Seq.iter (fun x -> x.Enabled <- enabled)
    )

    do KnownPeers |> Seq.iter (fun x ->
        peerListBox.Items.Add(sprintf "%s (%d)" (x.Address.ToString()) x.Port) |> ignore
    )

    do add peerListBox

    do this.Dock <- DockStyle.Fill

